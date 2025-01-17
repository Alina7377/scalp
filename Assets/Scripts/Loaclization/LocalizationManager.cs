using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LocalizationManager: MonoBehaviour
{
    private char _fieldSeperator = ';';
    private char _lineSeperater = '|';    
    private string[,] _localization;
    private string[] _languages = {"rus","eng"}; 
    private int _currentLenguage = 1;
    private float _countDocuments;
    private float _countOpenDocuments;

    public  event Action OnChangeLanguage;

    public static LocalizationManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        // end of new code

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ResetCSV();        
        OnChangeLanguage?.Invoke();
        SetProgress();
    }

    public void SetProgress()
    {
        // �������� 1, ��� ��� ���������� ����� �������� Null
        _countDocuments = GetTagList("", false).Count - 1;
        _countOpenDocuments = GetTagList("f", false).Count - 1;
    }

    public float GetProgress() 
    {
        return Mathf.Round((_countOpenDocuments / _countDocuments) * 100);
    }

    public void ResetCSV() 
    {
        string newFile = ReadCSV();
        string[] records = newFile.Split(_lineSeperater);
        _localization = new string[records.Length, records[0].Split(_fieldSeperator).Length];
        for (int i = 0; i < records.Length - 1; i++)
        {
            string[] fields = records[i].Split(_fieldSeperator);
            if (fields[0] == "") continue;
            for (int j = 0; j < fields.Length; j++)
            {
                if (j == 0) fields[j] = fields[j].Replace("\n", string.Empty);
                _localization[i, j] = fields[j];
            }
        }
    }

    private string ReadCSV()
    {
        string filePath = Application.dataPath + "/Resources/Dictonary.csv";
        FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        StreamReader streamReader = new StreamReader(fileStream);
        string result = streamReader.ReadToEnd();

        fileStream.Close();
        return result;
    }

    public void SetLanguageSetting() 
    {
        _currentLenguage =int.Parse(Settings.Instance.GetParam("language"));
    }

    public string GetTextForTag(string tag) 
    {
        if (_localization != null)
        {
            //���������� ����
            // ���� ���� ����� ������, ���� � ��� ������� ������ � CSV � _languages ���������
            int numLeng = 0;
            for (int i = 1; i < _localization.GetLength(1); i++)
            {
                if (_localization[0, i] == _languages[_currentLenguage])
                {
                    numLeng = i;
                    break;
                }
            }
            //���������� �������� �� ����
            if (numLeng > 0)
            {
                for (int i = 1; i < _localization.GetLength(0); i++)
                {
                    if (_localization[i, 0].Contains(tag))
                    {
                        return _localization[i, numLeng].TrimStart('"').TrimEnd('"');
                    }
                }
            }           
        }
        return tag;
    }

    public void Change() 
    {
        _currentLenguage++;
        if (_currentLenguage== _languages.Length) {_currentLenguage = 0;}
        Settings.Instance.SetParam("language", _currentLenguage.ToString());
        OnChangeLanguage?.Invoke();
    }

    /// <summary>
    /// ��������� ���� �����, ������� ������������ (equal=true) ��� �� ������������ (equal=false) avail
    /// </summary>
    /// <param name="avail"> ��� ����������� - f - �� ��������, t - ��������, n - �����</param>
    /// <param name="equal"> ����������� ��������� ��������� avail ��� �� ���������</param>
    /// <returns></returns>
    public List<string> GetTagList(string avail, bool equal)
    {
        List<string> list = new List<string>();
       // if(_localization !=null)
        for (int i = 1; i < _localization.GetLength(0); i++)
        {
            if (_localization[i, 1] != "" && (_localization[i, 1] == avail) == equal) list.Add(_localization[i, 0]);
        }
        return list;
    }


    /// <summary>
    /// �������� �� ����������� ���� (���� �� �����, �� ����� �����������)
    /// </summary>
    /// <param name="tag"> ���</param>
    /// <param name="avail"> ��� ����������� - f - �� ��������, t - ��������, n - �����</param>
    /// <param name="equal"> ����������� ��������� �������� avail ��� �� ���������</param>
    /// <returns></returns>
    public bool CheackAvail(string tag,string avail, bool equal) 
    {
        for (int i = 1; i < _localization.GetLength(0); i++)
            {
                if (_localization[i, 0].Contains(tag) && (_localization[i, 1]==avail) == equal && _localization[i, 1]!="") return true;   
            }
        return false;
    }

    public void WriteAvailForTag(string tag, string avail) 
    {
        for (int i = 1; i < _localization.GetLength(0); i++)
        {
            if (_localization[i, 0].Contains(tag)) 
            {
                _localization[i, 1] = avail;
                return; 
            }
        }
    }

    public void SafeCSV() 
    {
        string line = "";
        for (int i = 0; i < _localization.GetLength(0); i++)
        {
            for (int j = 0; j < _localization.GetLength(1); j++)            
            {
                if (j == _localization.GetLength(1) - 1) line = line + _localization[i, j];
                else line = line + _localization[i, j] + _fieldSeperator;
            }
            if (i< _localization.GetLength(0)-1) line = line + _lineSeperater;
        }
        File.WriteAllText(Application.dataPath + "/Resources/Dictonary.csv", line);
        Debug.Log("���� �����������!");
    }
}

