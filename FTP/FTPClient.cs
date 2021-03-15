using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class FTPClient
{

    //ip фтп-сервера
    private string host;
    //логин
    private string user;
    //пароль
    private string pass;
    // пассивные запросы
    private bool passive;
    // безопасное подключение SSL
    private bool ssl = false;

    FtpWebRequest ftpRequest; // объект для запросов
    FtpWebResponse ftpResponse; // объект для ответов от сервера 

    public FTPClient(string host, string user, string pass, bool passive)
    {
        this.host = host;
        this.user = user;
        this.pass = pass;
        this.passive = passive;
    }

    public int ListDirectory(string path)
    {
        if (path == null || path == "")
        {
            path = "/";
        }
        
        ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://" + host + path);
        ftpRequest.Credentials = new NetworkCredential(user, pass);
        ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
        ftpRequest.UsePassive = passive;
        ftpRequest.EnableSsl = ssl;

        // ответ от сервера
        try
        {
            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
        }
        catch(WebException e)
        {
            Console.WriteLine();
            return (int) ((FtpWebResponse)e.Response).StatusCode;
        }

        //переменная для хранения всей полученной информации
        string content = "";

        StreamReader sr = new StreamReader(ftpResponse.GetResponseStream(), System.Text.Encoding.ASCII);
        content = sr.ReadToEnd();
        sr.Close();
        ftpResponse.Close();

        Console.WriteLine("Contents of path "+path+" :");
        Console.WriteLine();
        Console.WriteLine(content);


        return 1;
        //DirectoryListParser parser = new DirectoryListParser(content);
        //return parser.FullListing;
    }

    public int DownloadFile(string path, string fileName)
    {

        ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://" + host + path + "/" + fileName);

        ftpRequest.Credentials = new NetworkCredential(user, pass);
        ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
        ftpRequest.UsePassive = passive;
        ftpRequest.EnableSsl = ssl;

        try
        {
            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
        }
        catch (WebException e)
        {
            Console.WriteLine();
            return (int)((FtpWebResponse)e.Response).StatusCode;
        }
        // ответ сервера
        Stream responseStream = ftpResponse.GetResponseStream();

        FileStream downloadedFile = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);

        // буфер для считываемых данных
        byte[] buffer = new byte[1024];
        int size = 0;

        while ((size = responseStream.Read(buffer, 0, 1024)) > 0)
        {
            downloadedFile.Write(buffer, 0, size);

        }
        ftpResponse.Close();
        downloadedFile.Close();
        responseStream.Close();

        return 1;
    }

    public int UploadFile(string path, string fileName)
    {
        //для имени файла
        string shortName = fileName.Remove(0, fileName.LastIndexOf("\\") + 1);

        FileStream uploadedFile = new FileStream(fileName, FileMode.Open, FileAccess.Read);

        ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://" + host + path + shortName);
        ftpRequest.Credentials = new NetworkCredential(user, pass);
        ftpRequest.EnableSsl = ssl;
        ftpRequest.UsePassive = passive;
        ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

        //Буфер для загружаемых данных
        byte[] file_to_bytes = new byte[uploadedFile.Length];
        //Считываем данные в буфер
        uploadedFile.Read(file_to_bytes, 0, file_to_bytes.Length);

        uploadedFile.Close();

        //Поток для загрузки файла 
        Stream writer;
        try
        {
            writer = ftpRequest.GetRequestStream();
        }
        catch (WebException e)
        {
            Console.WriteLine();
            return (int)((FtpWebResponse)e.Response).StatusCode;
        }

        writer.Write(file_to_bytes, 0, file_to_bytes.Length);
        writer.Close();

        return 1;
    }

    public int DeleteFile(string path)
    {
        ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://" + host + path);
        ftpRequest.Credentials = new NetworkCredential(user, pass);
        ftpRequest.EnableSsl = ssl;
        ftpRequest.UsePassive = passive;
        ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;

        try
        {
            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
        }
        catch (WebException e)
        {
            Console.WriteLine();
            return (int)((FtpWebResponse)e.Response).StatusCode;
        }

        ftpResponse.Close();

        return 1;
    }


    public int CreateDirectory(string path, string folderName)
    {
        FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://" + host + path + folderName);

        ftpRequest.Credentials = new NetworkCredential(user, pass);
        ftpRequest.EnableSsl = ssl;
        ftpRequest.UsePassive = passive;
        ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;

        try
        {
            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
        }
        catch (WebException e)
        {
            Console.WriteLine();
            return (int)((FtpWebResponse)e.Response).StatusCode;
        }

        ftpResponse.Close();

        return 1;
    }

    public int RemoveDirectory(string path)
    {
        string filename = path;
        FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://" + host + path);
        ftpRequest.Credentials = new NetworkCredential(user, pass);
        ftpRequest.EnableSsl = ssl;
        ftpRequest.UsePassive = passive;
        ftpRequest.Method = WebRequestMethods.Ftp.RemoveDirectory;

        try
        {
            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
        }
        catch (WebException e)
        {
            Console.WriteLine();
            return (int)((FtpWebResponse)e.Response).StatusCode;
        }

        ftpResponse.Close();

        return 1;
    }
}
