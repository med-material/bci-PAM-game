using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Globalization;

public enum LogMode
{
    Append,
    Overwrite
}

public class LogCollection
{
    public string label;
    public int count = 0;
    public bool saveHeaders = true;
    public Dictionary<string, Dictionary<int, object>> log = new Dictionary<string, Dictionary<int, object>>();
}

public class LoggingManager : MonoBehaviour
{
    private Dictionary<string, string> statelogs = new Dictionary<string, string>();
    private Dictionary<string, Dictionary<int, string>> logs = new Dictionary<string, Dictionary<int, string>>();

    // sampleLog[COLUMN NAME][COLUMN NO.] = [OBJECT] (fx a float, int, string, bool)
    private Dictionary<string, LogCollection> collections = new Dictionary<string, LogCollection>();

    [Header("Logging Settings")]
    [Tooltip("The Meta Collection will contain a session ID, a device ID and a timestamp.")]
    [SerializeField]
    private bool CreateMetaCollection = true;

    [Header("MySQL Save Settings")]
    [SerializeField]
    private bool enableMySQLSave = true;
    [SerializeField]
    private string email = "anonymous";

    [SerializeField]
    private ConnectToMySQL connectToMySQL;


    [Header("CSV Save Settings")]
    [SerializeField]
    private bool enableCSVSave = true;

    [Tooltip("If save path is empty, it defaults to My Documents.")]
    [SerializeField]
    private string savePath = "";

    [SerializeField]
    private string filePrefix = "log";

    [SerializeField]
    private string fileExtension = ".csv";

    private string filePath;
    private char fieldSeperator = ',';
    private string sessionID = "";
    private string deviceID = "";
    private string filestamp;

    // Start is called before the first frame update
    void Awake()
    {
        filestamp = GetTimeStamp().Replace('/', '-').Replace(":", "-");
        if (CreateMetaCollection)
        {
            GenerateUIDs();
            Log("Meta", "SessionID", sessionID, LogMode.Overwrite);
            Log("Meta", "DeviceID", deviceID, LogMode.Overwrite);
        }
        if (savePath == "")
        {
            savePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        }
    }

    public void GenerateUIDs()
    {
        sessionID = Md5Sum(System.DateTime.Now.ToString(SystemInfo.deviceUniqueIdentifier + "yyyy:MM:dd:HH:mm:ss.ffff").Replace(" ", "").Replace("/", "").Replace(":", ""));
        deviceID = SystemInfo.deviceUniqueIdentifier;
    }

    public Dictionary<string, Dictionary<int, object>> GetLog(string collectionLabel)
    {
        return new Dictionary<string, Dictionary<int, object>>(collections[collectionLabel].log);
    }

    public void SaveAllLogs()
    {
        foreach (KeyValuePair<string, LogCollection> pair in collections)
        {
            SaveLog(pair.Value.label);
        }
    }

    public void SaveLog(string collectionLabel)
    {
        if (collections.ContainsKey(collectionLabel))
        {
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                SaveToCSV(collectionLabel);
            }
            SaveToSQL(collectionLabel);
        }
        else
        {
            Debug.LogError("No Collection Called " + collectionLabel);
        }
    }

    public void CreateLog(string collectionLabel)
    {
        collections.Add(collectionLabel, new LogCollection());
    }

    public void Log(string collectionLabel, Dictionary<string, object> logData, LogMode logMode = LogMode.Append)
    {
        if (!collections.ContainsKey(collectionLabel))
        {
            collections.Add(collectionLabel, new LogCollection());
            collections[collectionLabel].label = collectionLabel;
        }
        foreach (KeyValuePair<string, object> pair in logData)
        {
            if (!collections[collectionLabel].log.ContainsKey(pair.Key))
            {
                collections[collectionLabel].log.Add(pair.Key, new Dictionary<int, object>());

                if (!collections[collectionLabel].log.ContainsKey("Timestamp"))
                {
                    collections[collectionLabel].log["Timestamp"] = new Dictionary<int, object>();
                }
                if (!collections[collectionLabel].log.ContainsKey("Framecount"))
                {
                    collections[collectionLabel].log["Framecount"] = new Dictionary<int, object>();
                }
                if (!collections[collectionLabel].log.ContainsKey("SessionID"))
                {
                    collections[collectionLabel].log["SessionID"] = new Dictionary<int, object>();
                }
                if (!collections[collectionLabel].log.ContainsKey("Email"))
                {
                    collections[collectionLabel].log["Email"] = new Dictionary<int, object>();
                }
            }
            int count = collections[collectionLabel].count;
            if (logMode == LogMode.Append)
            {
                if (collections[collectionLabel].log[pair.Key].ContainsKey(count))
                {
                    collections[collectionLabel].count++;
                    count = collections[collectionLabel].count;
                }
            }

            collections[collectionLabel].log["Timestamp"][count] = GetTimeStamp();
            collections[collectionLabel].log["Framecount"][count] = GetFrameCount();
            collections[collectionLabel].log["SessionID"][count] = sessionID;
            collections[collectionLabel].log["Email"][count] = email;
            collections[collectionLabel].log[pair.Key][count] = pair.Value;
        }
    }

    public void Log(string collectionLabel, string columnLabel, object value, LogMode logMode = LogMode.Append)
    {
        if (!collections.ContainsKey(collectionLabel))
        {
            collections.Add(collectionLabel, new LogCollection());
            collections[collectionLabel].label = collectionLabel;
        }

        if (!collections[collectionLabel].log.ContainsKey(columnLabel))
        {
            collections[collectionLabel].log.Add(columnLabel, new Dictionary<int, object>());

            if (!collections[collectionLabel].log.ContainsKey("Timestamp"))
            {
                collections[collectionLabel].log["Timestamp"] = new Dictionary<int, object>();
            }
            if (!collections[collectionLabel].log.ContainsKey("Framecount"))
            {
                collections[collectionLabel].log["Framecount"] = new Dictionary<int, object>();
            }
            if (!collections[collectionLabel].log.ContainsKey("SessionID"))
            {
                collections[collectionLabel].log["SessionID"] = new Dictionary<int, object>();
            }
            if (!collections[collectionLabel].log.ContainsKey("Email"))
            {
                collections[collectionLabel].log["Email"] = new Dictionary<int, object>();
            }
        }

        int count = collections[collectionLabel].count;
        if (logMode == LogMode.Append)
        {
            if (collections[collectionLabel].log[columnLabel].ContainsKey(count))
            {
                collections[collectionLabel].count++;
                count = collections[collectionLabel].count;
            }
        }

        collections[collectionLabel].log["Timestamp"][count] = GetTimeStamp();
        collections[collectionLabel].log["Framecount"][count] = GetFrameCount();
        collections[collectionLabel].log["SessionID"][count] = sessionID;
        collections[collectionLabel].log["Email"][count] = email;
        collections[collectionLabel].log[columnLabel][count] = value;
    }

    public void ClearAllLogs()
    {
        foreach (KeyValuePair<string, LogCollection> pair in collections)
        {
            collections[pair.Key].log.Clear();
            collections[pair.Key].count = 0;
        }
    }

    public void ClearLog(string collectionLabel)
    {
        if (collections.ContainsKey(collectionLabel))
        {
            collections[collectionLabel].log.Clear();
            collections[collectionLabel].count = 0;
        }
        else
        {
            Debug.LogError("Collection " + collectionLabel + " does not exist.");
            return;
        }
    }

    // Formats the logs to a CSV row format and saves them. Calls the CSV headers generation beforehand.
    // If a parameter doesn't have a value for a given row, uses the given value given previously (see 
    // UpdateHeadersAndDefaults).
    private void SaveToCSV(string label)
    {
        if (!enableCSVSave) return;
        string headerLine = "";
        if (collections[label].saveHeaders)
        {
            headerLine = GenerateHeaders(collections[label]);
        }
        object temp;
        string filename = collections[label].label;
        string filePath = savePath + "/" + filePrefix + filestamp + filename + fileExtension;
        using (var file = new StreamWriter(filePath, true))
        {
            if (collections[label].saveHeaders)
            {
                file.WriteLine(headerLine);
                collections[label].saveHeaders = false;
            }
            for (int i = 0; i <= collections[label].count; i++)
            {
                string line = "";
                foreach (KeyValuePair<string, Dictionary<int, object>> log in collections[label].log)
                {
                    if (line != "")
                    {
                        line += fieldSeperator;
                    }

                    if (log.Value.TryGetValue(i, out temp))
                    {
                        line += ConvertToString(temp);
                    }
                    else
                    {
                        line += "NULL";
                    }
                }
                file.WriteLine(line);
            }
        }
        Debug.Log(label + " logs with " + collections[label].count + 1 + " rows saved to " + savePath);
    }


    // Generates the headers in a CSV format and saves them to the CSV file
    private string GenerateHeaders(LogCollection collection)
    {
        string headers = "";
        foreach (string key in collection.log.Keys)
        {
            if (headers != "")
            {
                headers += fieldSeperator;
            }
            headers += key;
        }
        return headers;
    }

    private void SaveToSQL(string label)
    {
        if (!enableMySQLSave) { return; }

        if (!collections.ContainsKey(label))
        {
            Debug.LogError("Could not find collection " + label + ". Aborting.");
            return;
        }

        if (collections[label].log.Keys.Count == 0)
        {
            Debug.LogError("Collection " + label + " is empty. Aborting.");
            return;
        }

        connectToMySQL.AddToUploadQueue(collections[label].log, collections[label].label);
        connectToMySQL.UploadNow();
    }

    public string Md5Sum(string strToEncrypt)
    {
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);

        // encrypt bytes
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);

        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }

        return hashString.PadLeft(32, '0');
    }

    // Converts the values of the parameters (in a "object format") to a string, formatting them to the
    // correct format in the process.
    private string ConvertToString(object arg)
    {
        if (arg is float)
        {
            return ((float)arg).ToString("0.0000").Replace(",", ".");
        }
        else if (arg is int)
        {
            return arg.ToString();
        }
        else if (arg is bool)
        {
            return ((bool)arg) ? "TRUE" : "FALSE";
        }
        else if (arg is Vector3)
        {
            return ((Vector3)arg).ToString("0.0000").Replace(",", ".");
        }
        else
        {
            return arg.ToString();
        }
    }

    // Returns a time stamp including the milliseconds.
    private string GetTimeStamp()
    {
        return System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff");
    }

    private string GetFrameCount()
    {
        return Time.frameCount == null ? "-1" : Time.frameCount.ToString();
    }

}



/*using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System;

public class LoggingManager : MonoBehaviour
{
    static int condition;
    static int order;
    string filepath;
    string filename;
    string sep = ",";

    bool bciMode;

    private Dictionary<string, List<string>> logCollection;

    // Start is called before the first frame update
    void Start()
    {
        condition = GameObject.FindGameObjectWithTag("GameController").GetComponent<InputBlocks>().condition;
        order = GameObject.FindGameObjectWithTag("GameController").GetComponent<InputBlocks>().order;

        filename = "0" + condition + "_0" + order + "_playthroughdata";

        filepath = Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/MTA201039Data";
        Directory.CreateDirectory(filepath);

        logCollection = new Dictionary<string, List<string>>();
        logCollection["Date"] = new List<string>();
        logCollection["Timestamp"] = new List<string>();
        logCollection["Event"] = new List<string>();

        logCollection["NarrativeEvent"] = new List<string>();
        logCollection["InputBlockNo"] = new List<string>();
        logCollection["InputNo"] = new List<string>();
        logCollection["TrialNumber"] = new List<string>();
        logCollection["FeedbackType"] = new List<string>();
        logCollection["BlockResult"] = new List<string>();
        logCollection["WinCount"] = new List<string>();
        logCollection["LossCount"] = new List<string>();
        logCollection["Lane"] = new List<string>();
        logCollection["Column"] = new List<string>();
        logCollection["FishType"] = new List<string>();

        logCollection["KeyCode"] = new List<string>();
        logCollection["SequenceTime_ms"] = new List<string>();
        logCollection["TimeSinceLastKey_ms"] = new List<string>();
        logCollection["KeyOrder"] = new List<string>();
        logCollection["KeyType"] = new List<string>();
        logCollection["ExpectedKey1"] = new List<string>();
        logCollection["ExpectedKey2"] = new List<string>();
        logCollection["SequenceNumber"] = new List<string>();
        logCollection["SequenceComposition"] = new List<string>();
        logCollection["SequenceSpeed"] = new List<string>();
        logCollection["SequenceValidity"] = new List<string>();
        logCollection["SequenceType"] = new List<string>();
        logCollection["SequenceWindowClosure"] = new List<string>();
        logCollection["TargetFabInputRate"] = new List<string>();
        logCollection["TargetRecognitionRate"] = new List<string>();
        logCollection["StartPolicyReview"] = new List<string>();
        logCollection["Trials"] = new List<string>();
        logCollection["InterTrialIntervalSeconds"] = new List<string>();
        logCollection["InputWindowSeconds"] = new List<string>();
        logCollection["GameState"] = new List<string>();
        logCollection["GamePolicy"] = new List<string>();
        logCollection["CurrentRecognitionRate"] = new List<string>();
        logCollection["FabAlarmFixationPoint"] = new List<string>();
        logCollection["FabAlarmVariability"] = new List<string>();
        logCollection["CurrentFabRate"] = new List<string>();
        logCollection["CurrentFabAlarm"] = new List<string>();
        logCollection["InputConfidence"] = new List<string>();
        logCollection["InputValidity"] = new List<string>();
        logCollection["InputType"] = new List<string>();
        logCollection["InputNumber"] = new List<string>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void onKeySequenceLogReady(SequenceData sequenceData, InputData inputData) {
        logCollection["InputConfidence"].Add(inputData.confidence.ToString());
        logCollection["InputValidity"].Add(System.Enum.GetName(typeof(InputValidity), inputData.validity));
        logCollection["InputType"].Add(System.Enum.GetName(typeof(InputType), inputData.type));
        logCollection["InputNumber"].Add(inputData.inputNumber.ToString());
       foreach (string key in sequenceData.keySequenceLogs.Keys)
        {
            logCollection[key].AddRange(sequenceData.keySequenceLogs[key]);
            //Debug.Log(sequenceData.keySequenceLogs[key].Count);
        }
        FillGameEventColumns();
        Fill("NarrativeEvent");
        FillKeys();
    }

    public void onMotorImageryLogReady(InputData inputData) {
        logCollection["Event"].Add("InputEvent");
        logCollection["Date"].Add(System.DateTime.Now.ToString("yyyy-MM-dd"));
        logCollection["Timestamp"].Add(System.DateTime.Now.ToString("HH:mm:ss.ffff"));
        logCollection["InputConfidence"].Add(inputData.confidence.ToString());
        logCollection["InputValidity"].Add(System.Enum.GetName(typeof(InputValidity), inputData.validity));
        logCollection["InputType"].Add(System.Enum.GetName(typeof(InputType), inputData.type));
        logCollection["InputNumber"].Add(inputData.inputNumber.ToString());
        FillKeys();
    }

    public void onGameStateChanged(GameData gameData) {
        logCollection["Event"].Add(System.Enum.GetName(typeof(GameState), gameData.gameState));
        logCollection["Date"].Add(System.DateTime.Now.ToString("yyyy-MM-dd"));
        logCollection["Timestamp"].Add(System.DateTime.Now.ToString("HH:mm:ss.ffff"));
        logCollection["TargetFabInputRate"].Add(gameData.fabInputRate.ToString());
        logCollection["TargetRecognitionRate"].Add(gameData.recognitionRate.ToString());
        logCollection["StartPolicyReview"].Add(gameData.startPolicyReview.ToString());
        logCollection["Trials"].Add(gameData.trials.ToString());
        logCollection["InterTrialIntervalSeconds"].Add(gameData.interTrialIntervalSeconds.ToString());
        logCollection["InputWindowSeconds"].Add(gameData.inputWindowSeconds.ToString());
        logCollection["GameState"].Add(System.Enum.GetName(typeof(GameState), gameData.gameState));
        logCollection["FabAlarmFixationPoint"].Add(gameData.noInputReceivedFabAlarm.ToString());
        logCollection["FabAlarmVariability"].Add(gameData.fabAlarmVariability.ToString());

        FillKeySequenceColumns();
        FillGameEventColumns();
        Fill("NarrativeEvent");
        FillKeys();

        if (gameData.gameState == GameState.Stopped) {
            SendLogs();
        }
    }

    public void FillKeySequenceColumns() {
        logCollection["KeyCode"].Add("NA");
        logCollection["SequenceTime_ms"].Add("NA");
        logCollection["TimeSinceLastKey_ms"].Add("NA");
        logCollection["KeyOrder"].Add("NA");
        logCollection["KeyType"].Add("NA");
        logCollection["ExpectedKey1"].Add("NA");
        logCollection["ExpectedKey2"].Add("NA");
        logCollection["SequenceNumber"].Add("NA");
        logCollection["SequenceComposition"].Add("NA");
        logCollection["SequenceSpeed"].Add("NA");
        logCollection["SequenceValidity"].Add("NA");
        logCollection["SequenceType"].Add("NA");
        logCollection["SequenceWindowClosure"].Add("NA");
    }

    public void FillGameEventColumns()
    {
        logCollection["FeedbackType"].Add("NA");
        logCollection["BlockResult"].Add("NA");
        logCollection["WinCount"].Add("NA");
        logCollection["LossCount"].Add("NA");
        logCollection["FishType"].Add("NA");
        
        if (!bciMode)
        {
            logCollection["InputNo"].Add("NA");
            logCollection["InputBlockNo"].Add("NA");
            logCollection["TrialNumber"].Add("NA");
        }
    }

    public void FillEventColumn()
    {
        logCollection["Event"].Add("[gameevent]");
    }

    public void Fill(string toFill)
    {
        logCollection[toFill].Add("NA");
    }

    public void onGamePolicyChanged(GamePolicyData gamePolicyData) {
        //logCollection["Event"].Add("NewPolicy" + System.Enum.GetName(typeof(GamePolicy), gamePolicyData.gamePolicy));
        //logCollection["Date"].Add(System.DateTime.Now.ToString("yyyy-MM-dd"));
        //logCollection["Timestamp"].Add(System.DateTime.Now.ToString("HH:mm:ss.ffff"));
        //logCollection["GamePolicy"].Add(System.Enum.GetName(typeof(GamePolicy), gamePolicyData.gamePolicy));
        //FillKeySequenceColumns();
        //FillGameEventColumns();
        //FillKeys();
    }

    public void onGameDecision(GameDecisionData decisionData) {
        logCollection["Event"].Add("Decision" + System.Enum.GetName(typeof(InputTypes), decisionData.decision));
        logCollection["Date"].Add(System.DateTime.Now.ToString("yyyy-MM-dd"));
        logCollection["Timestamp"].Add(System.DateTime.Now.ToString("HH:mm:ss.ffff"));
        logCollection["CurrentRecognitionRate"].Add(decisionData.currentRecogRate.ToString());
        logCollection["CurrentFabRate"].Add(decisionData.currentRecogRate.ToString());
        logCollection["CurrentFabAlarm"].Add(decisionData.currentFabAlarm.ToString());
        // TODO: Whenever there is a GameDecision, we need to "Backfill" KeySequences.
        FillKeySequenceColumns();
        FillGameEventColumns();
        Fill("NarrativeEvent");
        FillKeys();
    }

    public void onInputWindowChanged(InputWindowState windowState) {
        logCollection["Event"].Add("InputWindow"+System.Enum.GetName(typeof(InputWindowState), windowState));
        logCollection["Date"].Add(System.DateTime.Now.ToString("yyyy-MM-dd"));
        logCollection["Timestamp"].Add(System.DateTime.Now.ToString("HH:mm:ss.ffff"));
        // TODO: Whenever there is a GameDecision, we need to "Backfill" KeySequences.
        FillKeySequenceColumns();
        FillGameEventColumns();
        Fill("NarrativeEvent");
        FillKeys();
    }

    public void onGameEventChanged(GameEventData gameEvent)
    {
        logCollection["Date"].Add(System.DateTime.Now.ToString("yyyy-MM-dd"));
        logCollection["Timestamp"].Add(System.DateTime.Now.ToString("HH:mm:ss.ffff"));
        logCollection["NarrativeEvent"].Add(gameEvent.narrativeEvent.ToString());
        logCollection["Lane"].Add(gameEvent.lane.ToString());
        logCollection["Column"].Add(gameEvent.column.ToString());
        if(gameEvent.narrativeEvent == NarrativeEvent.HookMoved)
        {
            FillGameEventColumns();
        }
        else if(gameEvent.narrativeEvent == NarrativeEvent.FishHooked)
        {
            bciMode = true;
            logCollection["InputNo"].Add(gameEvent.inputCounter.ToString());
            logCollection["InputBlockNo"].Add(gameEvent.blockCounter.ToString());
            logCollection["TrialNumber"].Add(gameEvent.trialCounter.ToString());
        }

        FillEventColumn();
        FillKeySequenceColumns();
        FillKeys();
    }

    public void onFeedback(GameEventData gameEvent)
    {
        logCollection["Date"].Add(System.DateTime.Now.ToString("yyyy-MM-dd"));
        logCollection["Timestamp"].Add(System.DateTime.Now.ToString("HH:mm:ss.ffff"));
        logCollection["NarrativeEvent"].Add(gameEvent.narrativeEvent.ToString());
        logCollection["Lane"].Add(gameEvent.lane.ToString());
        logCollection["Column"].Add(gameEvent.column.ToString());
        logCollection["FeedbackType"].Add(gameEvent.feedbackType.ToString());
        logCollection["InputNo"].Add(gameEvent.inputCounter.ToString());
        logCollection["InputBlockNo"].Add(gameEvent.blockCounter.ToString());
        logCollection["TrialNumber"].Add(gameEvent.trialCounter.ToString());

        FillEventColumn();
        FillKeySequenceColumns();
        FillKeys();
    }

    public void onEndOfBlock(GameEventData gameEvent)
    {
        logCollection["Date"].Add(System.DateTime.Now.ToString("yyyy-MM-dd"));
        logCollection["Timestamp"].Add(System.DateTime.Now.ToString("HH:mm:ss.ffff"));
        logCollection["NarrativeEvent"].Add(gameEvent.narrativeEvent.ToString());
        logCollection["BlockResult"].Add(gameEvent.blockResult.ToString());
        logCollection["InputBlockNo"].Add(gameEvent.blockCounter.ToString());
        logCollection["WinCount"].Add(gameEvent.winCounter.ToString());
        logCollection["LossCount"].Add(gameEvent.lossCounter.ToString());

        if (gameEvent.blockResult == BlockResult.Win)
        {
            logCollection["FishType"].Add(gameEvent.fishType);
        }

        bciMode = false;

        Fill("FeedbackType");
        Fill("Lane");
        Fill("Column");

        FillEventColumn();
        FillKeySequenceColumns();
        FillKeys();
    }

    public void onNextTrialStarted(GameEventData gameEvent)
    {
        logCollection["Date"].Add(System.DateTime.Now.ToString("yyyy-MM-dd"));
        logCollection["Timestamp"].Add(System.DateTime.Now.ToString("HH:mm:ss.ffff"));
        logCollection["NarrativeEvent"].Add(gameEvent.narrativeEvent.ToString());
        logCollection["InputNo"].Add(gameEvent.inputCounter.ToString());
        logCollection["InputBlockNo"].Add(gameEvent.blockCounter.ToString());
        logCollection["TrialNumber"].Add(gameEvent.trialCounter.ToString());

        FillEventColumn();
        FillKeySequenceColumns();
        FillKeys();

    }

    public void FillKeys() {
       foreach (string key in logCollection.Keys)
        {
            if (logCollection[key].Count < logCollection["Event"].Count) {
                string value;
                if (logCollection[key].Count > 0) {
                    value = logCollection[key][logCollection[key].Count-1];
                } else {
                    value = "NA";
                }
                var amount = logCollection["Event"].Count - logCollection[key].Count;
                if (amount > 0) {
                    for(int i = 0; i < amount; i++)
                    {
                        logCollection[key].Add(value);
                    }
                }
            }
        }
    }

    public void SendLogs() {
        LogToDisk();
        // TODO: Send to MySQL server.
    }

    public void LogToDisk() {
        if (logCollection["Event"].Count == 0) {
            Debug.Log("Nothing to log, returning..");
            return;
        }

        Debug.Log("Saving " + logCollection["Event"].Count + " Rows to " + filepath);
        string dest = filepath + "\\" + filename + "_" + System.DateTime.Now.ToString("yyyy-MM-dd-HH_mm_ss") + ".csv";

        // Log Header
        string[] keys = new string[logCollection.Keys.Count];
        logCollection.Keys.CopyTo(keys, 0);
        string dbCols = string.Join(sep, keys).Replace("\n", string.Empty);

        using (StreamWriter writer = File.AppendText(dest))
        {
            writer.WriteLine(dbCols);
        }

        // Create a string with the data
        List<string> dataString = new List<string>();
        for (int i = 0; i < logCollection["Event"].Count; i++)
        {
            List<string> row = new List<string>();
            foreach (string key in logCollection.Keys)
            {
                row.Add(logCollection[key][i]);
            }
            dataString.Add(string.Join(sep, row.ToArray()) + sep);
        }

        foreach (var log in dataString)
        {
            using (StreamWriter writer = File.AppendText(dest))
            {
                writer.WriteLine(log.Replace("\n", string.Empty));
            }
        }

        // Clear logCollection
       foreach (string key in logCollection.Keys)
        {
            
            logCollection[key].Clear();
        }
    }




}*/
