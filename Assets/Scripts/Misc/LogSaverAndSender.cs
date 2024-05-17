using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Net;
using System.Text;
using System;

public class LogSaverAndSender : MonoBehaviour
{
    public bool enableSave = true;
    public bool enableDiscord = true;

    private bool hasError = false;

    public string webhook_link = null;


    [Serializable]
    public struct Logs
    {
        public string condition;
        public string stackTrace;
        public LogType type;

        public string dateTime;
        

        public Logs(string condition, string stackTrace, LogType type, string dateTime)
        {
            this.condition = condition;
            this.stackTrace = stackTrace;
            this.type = type;
            this.dateTime = dateTime;
        }
    }

    [Serializable]
    public class LogInfo
    {
        public List<Logs> logInfoList = new List<Logs>();
    }

    LogInfo logs = new LogInfo();

    void OnEnable()
    {
        if (enableDiscord)
        {
            discordLog();
        }

        Application.logMessageReceived += LogCallback;
    }

    //Llamado al haber una excepción
    void LogCallback(string condition, string stackTrace, LogType type)
    {
        //Crear nuevo Log
        Logs logInfo = new Logs(condition, stackTrace, type, DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"));

        //Añadirlo a la lista
        logs.logInfoList.Add(logInfo);

        #if !UNITY_EDITOR
            if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
            {
                hasError = true;
            }
        #endif
    }

    private void discordLog()
    {
        LogInfo loadedData = DataSaver.loadData<LogInfo>("savelog");
        string date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");

        if (loadedData != null && loadedData.logInfoList != null
            && loadedData.logInfoList.Count > 0)
        {

            Debug.Log("Se encontró un Log!");

            //Convertir a json
            string messageToSend = "Date: " + date + "\n" + "Device Name: " + SystemInfo.deviceName + "\n" + "Game: " + Application.productName + "\n" + "Version: " + Application.version + "\n" + "OS: " + SystemInfo.operatingSystem + "\n" + "Logs:\n";

            string attachmentPath = Path.Combine(Application.persistentDataPath, "data");
            attachmentPath = Path.Combine(attachmentPath, "savelog.txt");

            SendFile(messageToSend, "savelog.txt", "txt", attachmentPath, "application/msexcel", "FaR-Logs", webhook_link);

            DataSaver.deleteData("savelog");
        }
    }

    public static string SendFile(
        string mssgBody,
        string filename,
        string fileformat,
        string filepath,
        string application,
        string userName,
        string webhook)
    {
        // Read file data
        FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
        byte[] data = new byte[fs.Length];
        fs.Read(data, 0, data.Length);
        fs.Close();

        // Generate post objects
        Dictionary<string, object> postParameters = new Dictionary<string, object>();
        postParameters.Add("filename", filename);
        postParameters.Add("fileformat", fileformat);
        postParameters.Add("file", new FormUpload.FileParameter(data, filename, application/*"application/msexcel"*/));

        postParameters.Add("username", userName);
        postParameters.Add("content", mssgBody);

        // Create request and receive response
        HttpWebResponse webResponse = FormUpload.MultipartFormDataPost(webhook, postParameters);

        // Process response
        StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
        string fullResponse = responseReader.ReadToEnd();
        webResponse.Close();
        Debug.Log("Discord: file success");

        //return string with response
        return fullResponse;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= LogCallback;
    }

    //Guardar el log cuando se pierda el foco
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && hasError)
        {
            //Guardar
            if (enableSave)
                DataSaver.saveData(logs, "savelog");
        }
    }

    //Guardar log al salir
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            //Guardar
            if (enableSave)
                DataSaver.saveData(logs, "savelog");
        }
    }
}

public static class FormUpload
{
    private static readonly Encoding encoding = Encoding.UTF8;
    public static HttpWebResponse MultipartFormDataPost(string postUrl, Dictionary<string, object> postParameters)
    {
        string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());

        string contentType = "multipart/form-data; boundary=" + formDataBoundary;

        byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);

        return PostForm(postUrl, contentType, formData);
    }

    private static HttpWebResponse PostForm(string postUrl, string contentType, byte[] formData)
    {
        HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;

        if (request == null)
        {
            Debug.LogWarning("request is not a http request");
            throw new NullReferenceException("request is not a http request");
        }

        // Configurar el tipo de petición
        request.Method = "POST";
        request.ContentType = contentType;
        request.CookieContainer = new CookieContainer();
        request.ContentLength = formData.Length;

        // Enviar los datos de la forma multipart/form-data
        using (Stream requestStream = request.GetRequestStream())
        {
            requestStream.Write(formData, 0, formData.Length);
            requestStream.Close();
        }

        return request.GetResponse() as HttpWebResponse;
    }

    private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
    {
        Stream formDataStream = new System.IO.MemoryStream();
        bool needsCLRF = false;

        foreach (var param in postParameters)
        {
            if (needsCLRF)
                formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

            needsCLRF = true;

            if (param.Value is FileParameter)
            {
                FileParameter fileToUpload = (FileParameter)param.Value;

                string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                    boundary,
                    param.Key,
                    fileToUpload.FileName ?? param.Key,
                    fileToUpload.ContentType ?? "application/octet-stream");

                formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

                formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
            }
            else
            {
                string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                    boundary,
                    param.Key,
                    param.Value);
                formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
            }
        }

        string footer = "\r\n--" + boundary + "--\r\n";
        formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

        // Tirar el Stream a un byte[]
        formDataStream.Position = 0;
        byte[] formData = new byte[formDataStream.Length];
        formDataStream.Read(formData, 0, formData.Length);
        formDataStream.Close();

        return formData;
    }

    public class FileParameter
    {
        public byte[] File { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public FileParameter(byte[] file) : this(file, null) { }
        public FileParameter(byte[] file, string filename) : this(file, filename, null) { }
        public FileParameter(byte[] file, string filename, string contenttype)
        {
            File = file;
            FileName = filename;
            ContentType = contenttype;
        }
    }
}
