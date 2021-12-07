using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class DataManager : MonoBehaviour
{
    #region Singleton
    private static DataManager _instance;
    public static DataManager Instance { get { return _instance; } }
    private void Awake()
    {
        _instance = this;
    }
    #endregion
    
    [SerializeField] TextAsset dataFile;

    private Stream rawDataStream;
    private Question[] data;

    private List<Question>[] categorizedQuestions;

    private List<string> categoryNames;

    private int[] categoryIndeces;

    public void LoadData(string key)
    {
        categorizedQuestions = new List<Question>[CommonData.Instance.numberOfCategories];
        categoryIndeces = new int[CommonData.Instance.numberOfCategories];
        for (int i = 0; i < CommonData.Instance.numberOfCategories; i++)
        {
            categorizedQuestions[i] = new List<Question>();
        }

        categoryNames = new List<string>(CommonData.Instance.categoryNames);

        string[] splitted = key.Split('.');
        byte[] keyArray = new byte[splitted.Length];
        for (int i = 0; i < keyArray.Length; i++)
        {
            keyArray[i] = byte.Parse(splitted[i]);
        }
        data = new Question[CommonData.Instance.numberOfAllQuestions];
        _ = DecryptDataAsync(keyArray);
    }

    private async Task DecryptDataAsync(byte[] key)
    {
        try
        {
            rawDataStream = new MemoryStream(dataFile.bytes);
            using Aes aes = Aes.Create();
            byte[] iv = new byte[aes.IV.Length];
            int numBytesToRead = aes.IV.Length;
            int numBytesRead = 0;
            while (numBytesToRead > 0)
            {
                int n = rawDataStream.Read(iv, numBytesRead, numBytesToRead);
                if (n == 0) break;

                numBytesRead += n;
                numBytesToRead -= n;
            }

            using CryptoStream cryptoStream = new CryptoStream(
               rawDataStream,
               aes.CreateDecryptor(key, iv),
               CryptoStreamMode.Read);
            using StreamReader decryptReader = new StreamReader(cryptoStream);

            for (int i = 0; i < CommonData.Instance.numberOfAllQuestions; i++)
            {
                data[i] = Question.Parse(await decryptReader.ReadLineAsync());
                if (categoryNames.IndexOf(data[i].category) == -1)
                {
                    Debug.Log("Category not found : " + data[i].category);
                }
                else
                {
                    categorizedQuestions[categoryNames.IndexOf(data[i].category)].Add(data[i]);
                }
            }
            LoadCategories();
            LoadingComplete();
        }
        catch (Exception e)
        {
            LoadingFailed(e);
        }
    }

    private void LoadingFailed(Exception e)
    {
        CommonData.Instance.loadingCanvas.SetActive(true);
    }

    private void LoadingComplete()
    {
        //int tempCounter = 0;
        //for (int i = 0; i < CommonData.Instance.numberOfCategories; i++)
        //{
            //Debug.Log(string.Format("Number of question in |{0}| : {1}", categoryNames[i], categorizedQuestions[i].Count));
            //tempCounter += categorizedQuestions[i].Count;
        //}
        //Debug.Log("All question count : " + tempCounter);
        CommonData.Instance.dataLoaded = true;
        if (CommonData.Instance.UILoaded && CommonData.Instance.gameStart)
        {
            CommonData.Instance.gameStart = false;
            MenuLogic.Instance.LoadingComplete();
        }
    }

    private void LoadCategories()
    {
        string fromPlayerPrefs = PlayerPrefs.GetString("cat", "0");
        if (fromPlayerPrefs.Equals("0")) //For first time
        {
            int[] arrayWithLengths = new int[CommonData.Instance.numberOfCategories];
            for (int i = 0; i < CommonData.Instance.numberOfCategories; i++)
            {
                arrayWithLengths[i] = categorizedQuestions[i].Count;
            }
            PlayerPrefs.SetString("cat", FormatCategoryIndeces(arrayWithLengths));
        }
        else
        {
            categoryIndeces = ParseCategoryIndeces(fromPlayerPrefs);
        }
    }
    
    public Question[] RandomQuestions(int category)
    {
        System.Random random = new System.Random();

        Question[] res = new Question[10];
        for (int i = 0; i < 10; i++)
        {
            if (categoryIndeces[category] < 10) categoryIndeces[category] = categorizedQuestions[category].Count - 1;
            int choosed = random.Next(categoryIndeces[category]);
            res[i] = categorizedQuestions[category][choosed];
            categoryIndeces[category]--;

            //swap
            Question temp = categorizedQuestions[category][choosed];
            categorizedQuestions[category][choosed] = categorizedQuestions[category][categoryIndeces[category]];
            categorizedQuestions[category][categoryIndeces[category]] = temp;
        }
        return res;
    }

    public Question[] GetSpecificQuestions(int[] indeces)
    {
        Question[] res = new Question[10];
        for (int i = 0; i < indeces.Length; i++)
        {
            res[i] = data[indeces[i]];
        }
        return res;
    }

    private string FormatCategoryIndeces(int[] indeces)
    {
        return string.Join("|", indeces);
    }

    private int[] ParseCategoryIndeces(string formatted)
    {
        return Array.ConvertAll(formatted.Split('|'), int.Parse);
    }

    public string NextID(string lastID)
    {
        char[] res = lastID.ToCharArray();
        bool remainder;
        for (int i = 6; i >= 0; i--)
        {
            if (lastID[i] == 90)
            {
                res[i] = (char)65;
                remainder = true;
            }
            else
            {
                res[i] = (char)(res[i] + 1);
                remainder = false;
            }

            if (!remainder) break;
        }
        return new string(res);
    }

    // Random questions from all questions
    //public Question[] RandomQuestions(int category)
    //{
    //    System.Random random = new System.Random();
    //    Question[] res = new Question[10];
    //    for (int i = 0; i < 10; i++)
    //    {
    //        res[i] = data[random.Next(CommonData.Instance.numberOfQuestions)];
    //    }
    //    return res;
    //}
}
