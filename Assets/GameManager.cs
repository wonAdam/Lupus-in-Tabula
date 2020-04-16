#pragma warning disable 0649
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using DG.Tweening;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Panel")]
    [SerializeField] GameObject _MainCanvas;
    [SerializeField] GameObject _ConnectionPanel;
    [SerializeField] GameObject _RoomPanel;
    [SerializeField] GameObject _ConfigPanel;
    [SerializeField] GameObject _VoteTogglePanel;
    [SerializeField] GameObject _GridPanel;
    [SerializeField] GameObject _ResultPanel;


    [Header("UI")]
    [SerializeField] Button _EnterBtn;
    [SerializeField] InputField _NameInputField;
    [SerializeField] Text _ConnectionText;
    [SerializeField] Text _RatioText;
    [SerializeField] Slider[] _JobSliders;
    [SerializeField] Button StartBtn;
    [SerializeField] Toggle _MediumToggle;
    [SerializeField] Text _TimerText;
    [SerializeField] Button _SendBtn;
    [SerializeField] TogglePanel _TogglePanelScript;
    [SerializeField] Button _RestartBtn;
    [SerializeField] AudioClip _MorningSound;
    [SerializeField] AudioClip _NightSound;
    [SerializeField] public AudioClip _ClickSound;
    [SerializeField] AudioClip _HumanWinSound;
    [SerializeField] AudioClip _DeadSound;
    [SerializeField] AudioClip _WolfWinSound;
    [SerializeField] AudioClip _HamsterWinSound;


    [Header("Prefab")]
    [SerializeField] GameObject _PlayerCardPrefab;
    [SerializeField] GameObject _MessagePanelPrefab;
    [SerializeField] GameObject _VoteTogglePrefab;

    [Header("Variable")]
    [SerializeField] string _RoomName = "room";
    [SerializeField] Sprite WerewolfImg;    
    [SerializeField] Sprite PossessedImg;    
    [SerializeField] Sprite HumanImg;    
    [SerializeField] Sprite SeerImg;    
    [SerializeField] Sprite BodyguardImg;    
    [SerializeField] Sprite MediumImg;    
    [SerializeField] Sprite FreemasonImg;  
    [SerializeField] Sprite WerehamsterImg;  
    [SerializeField] int FirstDaySec;
    [SerializeField] int NormalDaySec;
    [SerializeField] int NightSec;  

    public List<GameObject> _playerCardList;
    public List<string> _playerNicknameList;
    public List<string> _playerJobList;
    public List<bool> _playerAliveList;
    public List<int> _JobAliveleft;
    public List<int> _VoteResult;
    public List<int> BodyguardedIndex;
    public List<int> SeerPickHamsterIndex;
    public List<int> PossessedMeetIndex;
    public bool MediumExist;
    public int _MyIndex;
    public List<bool> _VoteDoneList;
    public State _CurrentState = State.Start_Config;

    public enum State {
         Start_Config, Start_Pause, Day_Vote, Night_Vote, 
         Ending_HumanWin, Ending_WolfWin, Ending_HamsterWin
    }

    public string WEREWOLF = "WEREWOLF";
    public string POSSESSED = "POSSESSED";
    public string HUMAN = "HUMAN";
    public string SEER = "SEER";
    public string MEDIUM = "MEDIUM";
    public string BODYGUARD = "BODYGUARD";
    public string FREEMASON = "FREEMASON";
    public string WEREHAMSTER = "WEREHAMSTER";




    [Header("Message")]
    [SerializeField] string Config_RatioNotMatch = "전체인원과 맞지않습니다.";


    #region Raise Event
    public override void OnEnable(){
        base.OnEnable();
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;

    }
    public override void OnDisable(){
        base.OnDisable();
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }

    const byte Config_SliderChange = 0;
    const byte Config_MediumToggleChange = 1;
    const byte Config_DistJob = 2;
    const byte Message_All = 3;
    const byte Timer_Set = 4;
    const byte LoadNextLevel = 5;
    const byte SendResult = 6;
    const byte WerewolfToggle = 7;
    const byte Restart = 8;




    private void NetworkingClient_EventReceived(EventData obj){

        if(obj.Code == Config_SliderChange){
            object[] data = (object[])obj.CustomData;
            int sliderindex = (int)data[0];
            int value = (int)data[1];
            _JobSliders[sliderindex].value = value;
            _JobSliders[sliderindex].transform.Find("Num").GetComponent<Text>().text = value.ToString();
            Refresh_ConfigRatioText();
        }
    
        else if(obj.Code == Config_MediumToggleChange){
            bool data = (bool)obj.CustomData;

            _MediumToggle.isOn = data;
        }

        else if(obj.Code == Config_DistJob){ //Others
            _ConfigPanel.SetActive(false);


            object[] data = (object[])obj.CustomData;
            string[] joblist = (string[])data[0];
            int[] jobleftlist = (int[])data[1];
            MediumExist = (bool)data[2];

            for(int i = 0; i < joblist.Length; i++){
                _playerJobList.Add(joblist[i]);
            }
            for(int i = 0; i < jobleftlist.Length; i++){
                _JobAliveleft.Add(jobleftlist[i]);
            }

            ShowMessage("당신은 " + _playerJobList[_MyIndex] + "입니다.", 0.4f);
            _CurrentState = State.Start_Pause;

        }
    
        else if(obj.Code == Message_All){ //ALL
            string data = (string)obj.CustomData;
            ShowMessage(data, 0.4f);
        }

        else if(obj.Code == Timer_Set){ //ALL
            int sec = (int)obj.CustomData;
            Set_Timer(sec);
        }
    
        else if(obj.Code == LoadNextLevel){ //ALL

            Load_NextLevel();

        }

        else if(obj.Code == SendResult){ //ALL


            object[] data = (object[]) obj.CustomData;
            int attackerIndex = (int)data[0];
            int victimIndex = (int)data[1];

            Debug.Log(attackerIndex + "이 " + victimIndex + "고름.");

            if(_CurrentState == State.Day_Vote){
                _VoteDoneList[attackerIndex] = true;

                _playerCardList[attackerIndex].transform.Find("Panel").Find("Votecall").
                        GetComponent<Text>().color = new Color(1f,0f,0f,1f);


                if(victimIndex >= 0){
                    _VoteResult[victimIndex] += 1;
                    _playerCardList[attackerIndex].transform.Find("Panel").Find("Votecall").
                        GetComponent<Text>().text = "\"" + _playerNicknameList[victimIndex] + "\"";
                }
                else{
                    _playerCardList[attackerIndex].transform.Find("Panel").Find("Votecall").
                        GetComponent<Text>().text = "\"" + "\"";
                }


            }
            //밤이고
            else if(_CurrentState == State.Night_Vote){

                _VoteDoneList[attackerIndex] = true;

                //뭔가를 골랐고
                if(victimIndex >= 0){
                    //늑대인간이 골랐으면
                    if(_playerJobList[attackerIndex] == WEREWOLF){

                        _VoteResult[victimIndex] += 1;
                        if(_playerJobList[_MyIndex] == WEREWOLF){
                            _playerCardList[attackerIndex].transform.Find("Panel").Find("Votecall").
                                GetComponent<Text>().color = new Color(1f,0f,0f,1f);
                            _playerCardList[attackerIndex].transform.Find("Panel").Find("Votecall").
                                GetComponent<Text>().text = "\"" + _playerNicknameList[victimIndex] + "\"";
                        }
                    }
                    //보디가드가 골랐으면
                    else if(_playerJobList[attackerIndex] == BODYGUARD)
                        BodyguardedIndex.Add(victimIndex);

                    //예언자가 골랐으면
                    else if(_playerJobList[attackerIndex] == SEER){
                        //그리고 그게 햄스터면
                        if(_playerJobList[victimIndex] == WEREHAMSTER){
                            SeerPickHamsterIndex.Add(victimIndex);
                        }
                    }
                    else if(_playerJobList[attackerIndex] == POSSESSED){
                        if(_playerJobList[victimIndex] == WEREWOLF){
                            PossessedMeetIndex.Add(attackerIndex);
                        }
                        
                        if(victimIndex == _MyIndex && _playerJobList[_MyIndex] == WEREWOLF){
                            _playerCardList[attackerIndex].transform.Find("Panel").Find("CardImg").
                                    GetComponent<Image>().color = new Color(1f,1f,1f,0.25f); 
                            string message = _playerNicknameList[attackerIndex] + "가 접선해왔습니다.";
                            ShowMessage(message, 0.4f);
                        }
                    }
                }

            }




            if(PhotonNetwork.IsMasterClient){
                IsVoteDone();
            }

        }

        else if(obj.Code == WerewolfToggle){

            object[] data = (object[])obj.CustomData;
            int attackerIndex = (int)data[0];
            int victimIndex = (int)data[1];

            if(_playerJobList[_MyIndex] == WEREWOLF){
                if(victimIndex >= 0){
                            _playerCardList[attackerIndex].transform.Find("Panel").Find("Votecall").
                                GetComponent<Text>().color = new Color(1f,1f,1f,1f);
                            _playerCardList[attackerIndex].transform.Find("Panel").Find("Votecall").
                                GetComponent<Text>().text = "\"" + _playerNicknameList[victimIndex] + "\"";
                }
                else{
                            _playerCardList[attackerIndex].transform.Find("Panel").Find("Votecall").
                                GetComponent<Text>().color = new Color(1f,1f,1f,1f);
                            _playerCardList[attackerIndex].transform.Find("Panel").Find("Votecall").
                                GetComponent<Text>().text = "\"" + "\"";
                }
            }

        }
    
        else if(obj.Code == Restart){

            for(int i =0; i < _playerCardList.Count; i++){
                Destroy(_playerCardList[i]);
            }
            _playerCardList.Clear();
            _playerNicknameList.Clear();
            _playerAliveList.Clear();
            _playerJobList.Clear();
            _JobAliveleft.Clear();

            _ResultPanel.SetActive(false);
            _GridPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);

            OnEnterRoom();
        }   
    }
    

    #endregion


    #region Connection State

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        Initialize_Panel();
    }
    private void Initialize_Panel(){
        _RoomPanel.SetActive(false);
        _ConfigPanel.SetActive(false);
        _ConnectionPanel.SetActive(true);
        _ConnectionText.text = "연결중입니다...";
        _NameInputField.gameObject.SetActive(false);
        _EnterBtn.gameObject.SetActive(false);
        _ResultPanel.SetActive(false);
        StartCoroutine(ConnectionPanel_Anim(1.5f));
    }
    public override void OnConnectedToMaster(){
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby(){
        _ConnectionText.text = "이름을 입력하세요.";
        _EnterBtn.gameObject.SetActive(true);
        _NameInputField.gameObject.SetActive(true);

    }
    public void OnClick_EnterBtn(){
        
        GetComponent<AudioSource>().volume = 1f;
        GetComponent<AudioSource>().PlayOneShot(_ClickSound);


        PhotonNetwork.NickName = _NameInputField.text;

        RoomOptions roomOptions = new RoomOptions{MaxPlayers = 20};
        

        PhotonNetwork.JoinOrCreateRoom(_RoomName, roomOptions, TypedLobby.Default);


    }
    public void OnValueChange_NameInput(){
        if(_NameInputField.text == ""){
            _EnterBtn.interactable = false;
        }
        else 
        _EnterBtn.interactable = true;
            }
    private IEnumerator ConnectionPanel_Anim(float speed){
        while(true){
            _ConnectionPanel.GetComponent<Image>().DOColor(new Color(0.15f, 0.15f, 0.15f), speed);
            yield return new WaitForSecondsRealtime(speed);
            _ConnectionPanel.GetComponent<Image>().DOColor(new Color(0, 0, 0), speed);
            yield return new WaitForSecondsRealtime(speed);
        }
    }
    public override void OnJoinRoomFailed(short returnCode, string message){

        RoomOptions roomOptions = new RoomOptions{MaxPlayers = 20};
        

        PhotonNetwork.CreateRoom(_RoomName, roomOptions, TypedLobby.Default);
    }
    public override void OnJoinedRoom(){
        OnEnterRoom();
    }
    private void OnEnterRoom(){
        StopCoroutine("ConnectionPanel_Anim");
        for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++){
            
            GameObject inst = Instantiate(_PlayerCardPrefab, _GridPanel.transform);
            inst.transform.Find("Panel").Find("Nickname").GetComponent<Text>().text = PhotonNetwork.PlayerList[i].NickName;
            inst.transform.Find("Panel").Find("Votecall").GetComponent<Text>().text = "";
            _playerCardList.Add(inst);
            _playerNicknameList.Add(PhotonNetwork.PlayerList[i].NickName);
            _playerAliveList.Add(true);

            if(PhotonNetwork.PlayerList[i].NickName == PhotonNetwork.NickName){
                inst.transform.Find("Panel").localScale = new Vector3(0.9f, 0.9f, 1f);
                _MyIndex = i;
            }
        }

        _RoomPanel.SetActive(true);
        _ConnectionPanel.SetActive(true);
        _ConfigPanel.SetActive(true);
        _SendBtn.gameObject.SetActive(false);


        if(PhotonNetwork.IsMasterClient){
           foreach(Slider slider in  _JobSliders){
               slider.interactable = true;
           }
            StartBtn.interactable = true;
            _MediumToggle.interactable = true;

            //for(int i = 0; i < _JobSliders.Length; i++){
            foreach(Slider slider in _JobSliders){
                slider.onValueChanged.AddListener((f) => OnValueChange_JobSlider(slider, (int)f));
            }
            _MediumToggle.onValueChanged.AddListener((t) => OnValueChange_MediumToggle(t));
        }
        else{
            foreach(Slider slider in  _JobSliders){
               slider.interactable = false;
           }
            StartBtn.interactable = false;
            _MediumToggle.interactable = false;
        }

        Refresh_ConfigRatioText();
    }
    
    #endregion








    #region RoomConfig_State

    public override void OnPlayerEnteredRoom(Player newPlayer){
            GameObject inst = Instantiate(_PlayerCardPrefab, _GridPanel.transform);
            inst.transform.Find("Panel").Find("Nickname").GetComponent<Text>().text = newPlayer.NickName;
            inst.transform.Find("Panel").Find("Votecall").GetComponent<Text>().text = "";
            _playerCardList.Add(inst);
            _playerNicknameList.Add(newPlayer.NickName);
            _playerAliveList.Add(true);

            Refresh_ConfigRatioText();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer){
        for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++){
            if(PhotonNetwork.PlayerList[i].NickName == otherPlayer.NickName){
                Destroy(_playerCardList[i]);
                _playerCardList.RemoveAt(i);
                _playerNicknameList.RemoveAt(i);
                _playerAliveList.RemoveAt(i);
                Refresh_ConfigRatioText();

                if(_CurrentState == State.Day_Vote || _CurrentState == State.Night_Vote){
                    _VoteDoneList[i] = true;
                    _TogglePanelScript._ToggleList.RemoveAt(i);
                    Destroy(_TogglePanelScript._ToggleList[i].gameObject);
                }

                return;
            }
        }
    }
    private void Refresh_ConfigRatioText(){

        int sum = 0;
        for(int i = 0; i< _JobSliders.Length; i++){
            sum += (int)_JobSliders[i].value;
        }

        _RatioText.text = sum.ToString() + " / " + PhotonNetwork.PlayerList.Length.ToString();


    }
    private void OnValueChange_JobSlider(Slider slider, int value){
        int sliderindex = new int();
        for(int i = 0 ; i < _JobSliders.Length; i++){
            if(_JobSliders[i] == slider) {
                sliderindex = i; break;
            }
        }

        object[] data = {sliderindex, value};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions{ Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions{Reliability = true};
        PhotonNetwork.RaiseEvent(Config_SliderChange, data, raiseEventOptions, sendOptions);
        Refresh_ConfigRatioText();
        slider.transform.Find("Num").GetComponent<Text>().text = value.ToString();

    }
    private void OnValueChange_MediumToggle(bool newValue){
        _JobSliders[5].value = 0;
        _JobSliders[5].transform.Find("Num").GetComponent<Text>().text = (0).ToString();

        OnValueChange_JobSlider(_JobSliders[5], 0);

        _JobSliders[5].interactable = newValue;

        object data = newValue;
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions{ Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions{Reliability = true};
        PhotonNetwork.RaiseEvent(Config_MediumToggleChange, data, raiseEventOptions, sendOptions);
        
        Refresh_ConfigRatioText();
    }
    public void OnClick_StartBtn(){

        GetComponent<AudioSource>().volume = 1f;
        GetComponent<AudioSource>().PlayOneShot(_ClickSound);
        
        if(CheckIfIsOkToStart()){
            _ConfigPanel.SetActive(false);
            JobDist();
        }
        else{
            ShowMessage(Config_RatioNotMatch, 0.4f);
        }

    }
    private void JobDist(){
        foreach(Slider slider in _JobSliders){
            _JobAliveleft.Add((int)slider.value);
            for(int i = 0; i < (int)slider.value; i++){
                _playerJobList.Add(slider.transform.Find("Job").GetComponent<Text>().text);
            }
        }

        for(int i = 0; i < _playerJobList.Count; i++){
            int swap_target_intdex = UnityEngine.Random.Range(0, _playerJobList.Count);
            string tmp = _playerJobList[i];
            _playerJobList[i] = _playerJobList[swap_target_intdex];
            _playerJobList[swap_target_intdex] = tmp;
        }

        MediumExist = _MediumToggle.isOn;

        object[] data = { _playerJobList.ToArray(), _JobAliveleft.ToArray(), MediumExist};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions{ Receivers = ReceiverGroup.Others };
        SendOptions sendOptions = new SendOptions{Reliability = true};
        PhotonNetwork.RaiseEvent(Config_DistJob, data, raiseEventOptions, sendOptions);

        ShowMessage("당신은 " + _playerJobList[_MyIndex] + "입니다.", 0.4f);
        Start_Timer(10);
        _CurrentState = State.Start_Pause;
    }
    private bool CheckIfIsOkToStart(){
        int sum = 0;
        foreach(Slider slider in _JobSliders){
            sum += (int)slider.value;
        }
        if(sum != PhotonNetwork.PlayerList.Length){ return false;}
        else return true;
    }
    private void SetPlayerCardImg(){


            for(int i = 0; i < _playerCardList.Count; i++){
                if(_playerJobList[i] == SEER)
                    _playerCardList[i].transform.Find("Panel").Find("CardImg").GetComponent<Image>().sprite = SeerImg;
                else if(_playerJobList[i] == HUMAN)
                    _playerCardList[i].transform.Find("Panel").Find("CardImg").GetComponent<Image>().sprite = HumanImg;
                else if(_playerJobList[i] == MEDIUM)
                    _playerCardList[i].transform.Find("Panel").Find("CardImg").GetComponent<Image>().sprite = MediumImg;
                else if(_playerJobList[i] == WEREHAMSTER)
                    _playerCardList[i].transform.Find("Panel").Find("CardImg").GetComponent<Image>().sprite = WerehamsterImg;
                else if(_playerJobList[i] == BODYGUARD)
                    _playerCardList[i].transform.Find("Panel").Find("CardImg").GetComponent<Image>().sprite = BodyguardImg;
                else if(_playerJobList[i] == POSSESSED)
                    _playerCardList[i].transform.Find("Panel").Find("CardImg").GetComponent<Image>().sprite = PossessedImg;
                else if(_playerJobList[i] == WEREWOLF)
                    _playerCardList[i].transform.Find("Panel").Find("CardImg").GetComponent<Image>().sprite = WerewolfImg;                
                else if(_playerJobList[i] == FREEMASON)
                    _playerCardList[i].transform.Find("Panel").Find("CardImg").GetComponent<Image>().sprite = FreemasonImg;
            }


        if(_playerJobList[_MyIndex] == WEREWOLF || _playerJobList[_MyIndex] == FREEMASON){
            for(int i = 0; i < _playerCardList.Count; i++){
                if(_playerJobList[i] == _playerJobList[_MyIndex]){
                    _playerCardList[i].transform.Find("Panel").Find("CardImg").GetComponent<Image>().color = new Color(1f,1f,1f,0.25f);
                }
            }
        }
        else{
            _playerCardList[_MyIndex].transform.Find("Panel").Find("CardImg").GetComponent<Image>().color = new Color(1f,1f,1f,0.25f);
        }
    }
    private void SetResultPanel(){

        _ResultPanel.transform.Find("JobResult").GetComponent<Text>().text = "";


        for(int i = 0; i < _playerJobList.Count; i++){

            _ResultPanel.transform.Find("JobResult").GetComponent<Text>().text += 
                _playerNicknameList[i] + " : " + _playerJobList[i];
                
            if(_playerJobList[i] == POSSESSED && PossessedMeetIndex.Contains(i)){
                _ResultPanel.transform.Find("JobResult").GetComponent<Text>().text += "(접선완료)";
            }

            _ResultPanel.transform.Find("JobResult").GetComponent<Text>().text += "\n";

        }
        PossessedMeetIndex.Clear();

    }

    #endregion












    #region Game
    

        #region Timer Methods
            private void Start_Timer(int sec){
                StopAllCoroutines();
                StartCoroutine(TimerTikToking(sec));
            }
            private IEnumerator TimerTikToking(int sec){
                int time_sec = sec;
                while(true){
                
                    object data = time_sec;
                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions{ Receivers = ReceiverGroup.All };
                    SendOptions sendOptions = new SendOptions{Reliability = true};
                    PhotonNetwork.RaiseEvent(Timer_Set, data, raiseEventOptions, sendOptions);

                    yield return new WaitForSecondsRealtime(1f);

                    time_sec--;

                    if(time_sec < 0) {

                        PhotonNetwork.RaiseEvent(LoadNextLevel, null, raiseEventOptions, sendOptions);
                        
                        break;

                    }
                }
            }    
            private void Set_Timer(int time){
                int minten = (time/600)%6;
                int minone = (time/60)%10;
                int secten = (time/10)%6;
                int secone = (time%10);

                _TimerText.text = minten.ToString() + minone.ToString() + " : " + secten.ToString() + secone.ToString();
            }

        #endregion

        #region Message Methods
            private void ShowMessage(string message, float speed){
                GameObject window = Instantiate(_MessagePanelPrefab, _MainCanvas.transform);
                window.transform.SetAsLastSibling();
                window.transform.Find("Message").GetComponent<Text>().text = message;
                window.GetComponent<RectTransform>().localScale = new Vector3(0f,0f,1f);
                window.GetComponent<RectTransform>().DOScale(new Vector3(1f,1f,1f), speed);
                window.GetComponentInChildren<Button>().onClick.AddListener(() => CloseMessage(window, 0.4f));
            }
            private void CloseMessage(GameObject Panel, float speed){
                Panel.GetComponent<RectTransform>().DOScale(new Vector3(0f,0f,1f), speed);
                Destroy(Panel, speed);
            }
        #endregion

    
    private void Load_NextLevel(State state = 0){


        if(state == 0){

            if(_CurrentState == State.Start_Pause){
                _CurrentState = State.Day_Vote;

                SetPlayerCardImg();

                if(CheckIfIsWinner()) return;
                
                Set4Day();

                if(PhotonNetwork.IsMasterClient)
                    Start_Timer(FirstDaySec);

            }
            else if(_CurrentState == State.Day_Vote){
                _CurrentState = State.Night_Vote;

                Execute4Day();
                
                if(CheckIfIsWinner()) return;

                Set4Night();

                if(PhotonNetwork.IsMasterClient)
                    Start_Timer(NightSec);

            }
            else if(_CurrentState == State.Night_Vote){
                _CurrentState = State.Day_Vote;

                Execute4Night();

                if(CheckIfIsWinner()) return;

                Set4Day();

                if(PhotonNetwork.IsMasterClient)
                    Start_Timer(NormalDaySec);

            }

        }
        else{
            StopAllCoroutines();
            SetResultPanel();
            _ResultPanel.transform.Find("ResultTitle").GetComponent<Text>().text = "";

            if(state == State.Ending_HumanWin){
            GetComponent<AudioSource>().volume = 1f;
            GetComponent<AudioSource>().PlayOneShot(_HumanWinSound);
            GetComponent<AudioSource>().DOFade(0f, 8f);
            GetComponent<AudioSource>().SetScheduledEndTime(8f);
            _ResultPanel.transform.Find("ResultTitle").GetComponent<Text>().text = 
                    "Human Win";
            }
            else if(state == State.Ending_WolfWin){
            GetComponent<AudioSource>().volume = 1f;
            GetComponent<AudioSource>().PlayOneShot(_WolfWinSound);
            GetComponent<AudioSource>().DOFade(0f, 8f);
            GetComponent<AudioSource>().SetScheduledEndTime(8f);
                _ResultPanel.transform.Find("ResultTitle").GetComponent<Text>().text = 
                    "Werewolf Win";
            }
            else if(state == State.Ending_HamsterWin){
            GetComponent<AudioSource>().volume = 1f;
            GetComponent<AudioSource>().PlayOneShot(_HamsterWinSound);
            GetComponent<AudioSource>().DOFade(0f, 8f);
            GetComponent<AudioSource>().SetScheduledEndTime(8f);
                _ResultPanel.transform.Find("ResultTitle").GetComponent<Text>().text = 
                    "Werehamster Win";
            }

            if(PhotonNetwork.IsMasterClient)
                _RestartBtn.gameObject.SetActive(true);
            else
                _RestartBtn.gameObject.SetActive(false);

            _ResultPanel.SetActive(true);
        }

    }

    private void Set4Day(){

        GetComponent<AudioSource>().volume = 1f;
        GetComponent<AudioSource>().PlayOneShot(_MorningSound);


        _GridPanel.GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f, 1f);

        //_voteResult, voteDone Set
        for(int i = 0; i < _playerCardList.Count; i++){
            if(_playerAliveList[i] == true)
                _VoteDoneList.Add(false);
            else
                _VoteDoneList.Add(true);

            _VoteResult.Add(0);
        }




        if(_playerAliveList[_MyIndex] == true){

            _SendBtn.gameObject.SetActive(true);

            _TogglePanelScript.InstantiateToggles(_playerNicknameList, _playerAliveList, _CurrentState);

        }
        else{

                _SendBtn.gameObject.SetActive(false);

                foreach (Toggle toggle in _TogglePanelScript._ToggleList)
                {
                    Destroy(toggle.gameObject);
                }

                _TogglePanelScript._ToggleList.Clear();
            
        }

    }

    public void OnClickSend(){

        // 결과 보내고 //toggle들없애고
        object[] data = {_MyIndex, _TogglePanelScript.WhatsOn_N_FinishVote() };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions{ Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions{Reliability = true};
        PhotonNetwork.RaiseEvent(SendResult, data, raiseEventOptions, sendOptions);


        //sendbtn 숨기고
        _SendBtn.gameObject.SetActive(false);

        if(_CurrentState == State.Night_Vote){
            int index = (int)data[1];

            if(index >= 0){
                if(_playerJobList[_MyIndex] == MEDIUM){
                    string message = _playerNicknameList[index] + "는 " + _playerJobList[index] + "입니다.";
                    ShowMessage(message, 0.4f); 
                }
                else if(_playerJobList[_MyIndex] == SEER){
                    if(_playerJobList[index] == WEREWOLF){
                        string message1 = _playerNicknameList[index] + "는 " + _playerJobList[index] + "입니다.";
                        ShowMessage(message1, 0.4f); 
                    }
                    else{
                        if(_playerJobList[index] == WEREHAMSTER){
                            string message1 = _playerNicknameList[index] + "는 " + WEREHAMSTER + "입니다.\n낮이 시작할때 죽습니다.";
                            ShowMessage(message1, 0.4f); 
                        }
                        else{
                            string message1 = _playerNicknameList[index] + "는 " + WEREWOLF + "가 아닙니다.";
                            ShowMessage(message1, 0.4f); 
                        }
                    }
                            
                }
                else if(_playerJobList[_MyIndex] == POSSESSED){

                    if(_playerJobList[index] == WEREWOLF){
                        string message1 = _playerNicknameList[index] + "에게 접선합니다.";
                        ShowMessage(message1, 0.4f);
                        _playerCardList[index].transform.Find("Panel").Find("CardImg").
                                GetComponent<Image>().color = new Color(1f,1f,1f,0.25f); 
                    }
                    else{
                        string message1 = _playerNicknameList[index] + "는 " + WEREWOLF + "가 아닙니다.";
                        ShowMessage(message1, 0.4f); 
                    }

                }

            }

        }


    }

    private void IsVoteDone(){

        //다 했는지 검사하고 

        for(int i = 0; i < _VoteDoneList.Count; i++){
            if(_VoteDoneList[i] == false){ return;}
        }

        //다했으면 Load RE
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions{ Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions{Reliability = true};
        PhotonNetwork.RaiseEvent(LoadNextLevel, null, raiseEventOptions, sendOptions);
    }

    private void Execute4Day(){

        //결과 실행

            //희생자 검사
        int max = 0; int index = -1;
        for(int i = 0; i < _VoteResult.Count; i++){

            if(_VoteResult[i] > max) {
                max = _VoteResult[i]; index = i;
            }

        }


            // 동점자 검사
        for(int i = 0; i < _VoteResult.Count; i++){
            if(_VoteResult[i] == max && i != index){
                //WrapUpResult_N_Done();
                _VoteResult.Clear();
                _VoteDoneList.Clear();

                //message
                string message1 = "동점자가 생겨\n아무도 죽지 않습니다.";
                ShowMessage(message1, 0.4f);

                //
                for(int j = 0 ; j < _playerCardList.Count; j++){

                    _playerCardList[j].transform.Find("Panel").Find("Votecall").
                                    GetComponent<Text>().text = "";

                }
                        
                return;
            }
        }

            //희생자 세팅

        GetComponent<AudioSource>().volume = 1f;
        GetComponent<AudioSource>().PlayOneShot(_DeadSound);
            
        _playerAliveList[index] = false;
        _playerCardList[index].transform.Find("Panel").Find("Nickname").
                        GetComponent<Text>().color = new Color(0.1f,0.1f,0.1f,1f);
        if(MediumExist == true){
            string message = _playerNicknameList[index] + "가 재판에 의해 죽었습니다.";
            ShowMessage(message, 0.4f);
        }
        else{
            string message = _playerNicknameList[index] + 
            "(" + _playerJobList[index] + ")" + "가\n 재판에 의해 죽었습니다.";
            ShowMessage(message, 0.4f);
        }


        //정리 voteresult, votedonelist, playerCard
        //WrapUpResult_N_Done();
        _VoteResult.Clear();
        _VoteDoneList.Clear();

        for(int i = 0 ; i < _playerCardList.Count; i++){

            _playerCardList[i].transform.Find("Panel").Find("Votecall").
                            GetComponent<Text>().text = "";

        }

    }

    private void Set4Night(){
        
        GetComponent<AudioSource>().volume = 1f;

        GetComponent<AudioSource>().PlayOneShot(_NightSound);
        GetComponent<AudioSource>().DOFade(0f, 8f);
        GetComponent<AudioSource>().SetScheduledEndTime(8f);

        _GridPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);

        //_voteResult, voteDone Set
        for(int i = 0; i < _playerCardList.Count; i++){

            if(_playerAliveList[i] == true && ( _playerJobList[i] == WEREWOLF || 
            _playerJobList[i] == SEER || _playerJobList[i] == MEDIUM || 
            _playerJobList[i] == BODYGUARD || _playerJobList[i] == POSSESSED))
                _VoteDoneList.Add(false);
            else if(_playerAliveList[i] == false || _playerJobList[i] == HUMAN || 
            _playerJobList[i] == FREEMASON || _playerJobList[i] == WEREHAMSTER )
                _VoteDoneList.Add(true);

            _VoteResult.Add(0);

        }



        if(_playerAliveList[_MyIndex] == true){

            if(_playerJobList[_MyIndex] == WEREWOLF ||
            _playerJobList[_MyIndex] == SEER ||
            _playerJobList[_MyIndex] == BODYGUARD ||
            _playerJobList[_MyIndex] == MEDIUM ||
            _playerJobList[_MyIndex] == POSSESSED)
            {

            _SendBtn.gameObject.SetActive(true);

            _TogglePanelScript.InstantiateToggles(_playerNicknameList, _playerAliveList, _CurrentState);
            }
            else{

                _SendBtn.gameObject.SetActive(false);

                foreach (Toggle toggle in _TogglePanelScript._ToggleList)
                {
                    Destroy(toggle.gameObject);
                }

                _TogglePanelScript._ToggleList.Clear();

            }

        }  
        else{

                _SendBtn.gameObject.SetActive(false);

                foreach (Toggle toggle in _TogglePanelScript._ToggleList)
                {
                    Destroy(toggle.gameObject);
                }

                _TogglePanelScript._ToggleList.Clear();

        }

        


    }

    private void Execute4Night(){

        //결과 실행
        bool isThereVictim = false;

            //예언자의 픽
        if(SeerPickHamsterIndex.Count > 0){

            _playerAliveList[SeerPickHamsterIndex[0]] = false;
            _playerCardList[SeerPickHamsterIndex[0]].transform.Find("Panel").Find("Nickname").
                            GetComponent<Text>().color = new Color(0.1f,0.1f,0.1f,1f);
            GetComponent<AudioSource>().volume = 1f;
            GetComponent<AudioSource>().PlayOneShot(_DeadSound);

                //message
                if(MediumExist == true){
                    string message1 = _playerNicknameList[SeerPickHamsterIndex[0]] + "가 지난 밤에 죽었습니다.";
                    ShowMessage(message1, 0.4f);
                }
                else{
                    string message1 = _playerNicknameList[SeerPickHamsterIndex[0]] + 
                    "(" + _playerJobList[SeerPickHamsterIndex[0]] + ")" +
                    "가\n 지난 밤에 죽었습니다.";
                    ShowMessage(message1, 0.4f);
                }

                isThereVictim = true;
                SeerPickHamsterIndex.Clear();
            
        }//if(SeerPickHamsterIndex >= 0)


            //희생자 검사
        int max = 0; int index = -1;
        for(int i = 0; i < _VoteResult.Count; i++){

            if(_VoteResult[i] > max) {
                max = _VoteResult[i]; index = i;
            }

        }

            
        //1표 이상이 있으면
        if(index >= 0){
            // 동점자 검사하자
            for(int i = 0; i < _VoteResult.Count; i++){
                //동점자가 있으면 아무도 안죽는데
                if(_VoteResult[i] == max && i != index){
                    
                    // 예언자한테 이미 한명죽었으면 정리하고 리턴
                    if(isThereVictim == true) {
                        
                        //WrapUpResult_N_Done();
                        _VoteResult.Clear();
                        _VoteDoneList.Clear();
                        BodyguardedIndex.Clear();
                        for(int j = 0 ; j < _playerCardList.Count; j++){

                            _playerCardList[j].transform.Find("Panel").Find("Votecall").
                                            GetComponent<Text>().text = "";

                        } // for(int j = 0 ; j < _playerCardList.Count; j++)
                        
                        return;
                        
                    }// if(isThereVictim == true)
                    // 예언자한테 아무도 안죽었으면 메세지 보내고 정리하고 리턴
                    else{

                        //WrapUpResult_N_Done();
                        _VoteResult.Clear();
                        _VoteDoneList.Clear();
                        BodyguardedIndex.Clear();

                        //message
                        string message1 = "아무도 죽지 않았습니다.";
                        ShowMessage(message1, 0.4f);

                        //
                        for(int j = 0 ; j < _playerCardList.Count; j++){

                            _playerCardList[j].transform.Find("Panel").Find("Votecall").
                                            GetComponent<Text>().text = "";

                        }
                        return;
                    }                        
                }//if(_VoteResult[i] == max && i != index)  
            }//for(int i = 0; i < _VoteResult.Count; i++)
        
        

            //동점자가 없는데

                    // 그게 쥐인간이면 안죽음
            if(_playerJobList[index] == WEREHAMSTER){
                    // 예언자가 죽였으면 정리하고 리턴
                if(isThereVictim == true) {
                        
                            //WrapUpResult_N_Done();
                            _VoteResult.Clear();
                            _VoteDoneList.Clear();
                            BodyguardedIndex.Clear();
                            for(int i = 0 ; i < _playerCardList.Count; i++){

                                _playerCardList[i].transform.Find("Panel").Find("Votecall").
                                                GetComponent<Text>().text = "";

                            }
                            
                            return;
                        
                } //if(isThereVictim == true)
                    // 예언자가 못죽였으면 메세지보내고 정리하고 리턴
                else{
                        //WrapUpResult_N_Done();
                        _VoteResult.Clear();
                        _VoteDoneList.Clear();
                        BodyguardedIndex.Clear();

                        //message
                        string message1 = "아무도 죽지 않았습니다.";
                        ShowMessage(message1, 0.4f);

                        //
                        for(int j = 0 ; j < _playerCardList.Count; j++){

                            _playerCardList[j].transform.Find("Panel").Find("Votecall").
                                            GetComponent<Text>().text = "";

                        }
                        return;                
                } // else
            } // if(_playerJobList[index] == WEREHAMSTER)


                    //그게 쥐인간이 아니면 죽어야지
            else{
                //근데 보디가드가 지켰으면 안죽어야지
                if(BodyguardedIndex.Contains(index)){

                        //예언자가 죽였으면 정리하고 리턴
                        if(isThereVictim == true){
                            //WrapUpResult_N_Done();
                            _VoteResult.Clear();
                            _VoteDoneList.Clear();
                            BodyguardedIndex.Clear();

                            for(int i = 0 ; i < _playerCardList.Count; i++){

                                _playerCardList[i].transform.Find("Panel").Find("Votecall").
                                                GetComponent<Text>().text = "";

                            }
                            
                            return;
                        }
                        //예언자가 못죽였으면 메세지보내고 정리하고 리턴
                        else{

                            //WrapUpResult_N_Done();
                            _VoteResult.Clear();
                            _VoteDoneList.Clear();
                            BodyguardedIndex.Clear();

                            //message
                            string message1 = "아무도 죽지 않았습니다.";
                            ShowMessage(message1, 0.4f);

                            //
                            for(int i = 0 ; i < _playerCardList.Count; i++){

                                _playerCardList[i].transform.Find("Panel").Find("Votecall").
                                                GetComponent<Text>().text = "";

                            }
                            
                            return;

                        }
                }// if(BodyguardedIndex == index)

                //보디가드가 못지켰으면 죽어야지
                else{

                    if(!isThereVictim){
                        GetComponent<AudioSource>().volume = 1f;
                        GetComponent<AudioSource>().PlayOneShot(_DeadSound);
                    }

                        _playerAliveList[index] = false;
                        _playerCardList[index].transform.Find("Panel").Find("Nickname").
                                        GetComponent<Text>().color = new Color(0.1f,0.1f,0.1f,1f);
                        if(MediumExist == true){
                            string message = _playerNicknameList[index] + "가 지난 밤에 죽었습니다.";
                            ShowMessage(message, 0.4f);
                        }
                        else{
                            string message = _playerNicknameList[index] + 
                            "(" + _playerJobList[index] + ")" + 
                            "가\n 지난 밤에 죽었습니다.";
                            ShowMessage(message, 0.4f);
                        }

                        //WrapUpResult_N_Done();
                        _VoteResult.Clear();
                        _VoteDoneList.Clear();
                        BodyguardedIndex.Clear();

                        for(int i = 0 ; i < _playerCardList.Count; i++){

                            _playerCardList[i].transform.Find("Panel").Find("Votecall").
                                            GetComponent<Text>().text = "";


                        }
                    return;

                } // else
            } // else

        }//if(index >= 0)
        
        //1표 이상 없고 (늑대인간이 아무도 안고름)       
        else{

            //예언자가 죽였으면 정리하고 리턴
            if(isThereVictim == true){
                //WrapUpResult_N_Done();
                _VoteResult.Clear();
                _VoteDoneList.Clear();
                BodyguardedIndex.Clear();

                for(int i = 0 ; i < _playerCardList.Count; i++){

                    _playerCardList[i].transform.Find("Panel").Find("Votecall").
                                    GetComponent<Text>().text = "";

                }
                            
                return;
            } // if(isThereVictim == true) //예언자가 죽였으면 정리하고 리턴
            //예언자가 못죽였으면 메세지보내고 정리하고 리턴
            else{

                //WrapUpResult_N_Done();
                _VoteResult.Clear();
                _VoteDoneList.Clear();
                BodyguardedIndex.Clear();

                //message
                string message1 = "아무도 죽지 않았습니다.";
                ShowMessage(message1, 0.4f);

                //
                for(int i = 0 ; i < _playerCardList.Count; i++){

                        _playerCardList[i].transform.Find("Panel").Find("Votecall").
                                    GetComponent<Text>().text = "";

                }
                            
                return;
            } // else //예언자가 못죽였으면 메세지보내고 정리하고 리턴

        } // else //1표 이상 없고 (늑대인간이 아무도 안고름)     
    } // endfunc

    private bool CheckIfIsWinner(){
        int wolfteam = 0;
        int wolf = 0;
        int humanteam = 0;
        int hamster = 0;
        for(int i =0; i < _playerCardList.Count; i++){

            if(_playerAliveList[i] == true){

                if(_playerJobList[i] == WEREWOLF || (_playerJobList[i] == POSSESSED && PossessedMeetIndex.Contains(i))){
                    wolfteam++;
                    if(_playerJobList[i] == WEREWOLF )
                        wolf++;
                }
                else if(_playerJobList[i] == SEER || _playerJobList[i] == HUMAN ||
                _playerJobList[i] == MEDIUM || _playerJobList[i] == BODYGUARD ||
                _playerJobList[i] == FREEMASON)
                    humanteam++;
                else if(_playerJobList[i] == WEREHAMSTER)
                    hamster++;

            }

        }


        if(wolfteam >= humanteam){
            if(hamster > 0){
                Load_NextLevel(State.Ending_HamsterWin);
            }
            else{
                Load_NextLevel(State.Ending_WolfWin);
            }
            return true;
        }
        else if(wolf == 0){
            if(hamster > 0){
                Load_NextLevel(State.Ending_HamsterWin);
            }
            else{
                Load_NextLevel(State.Ending_HumanWin);
            }
            return true;
        }
        return false;

    }

    public void WerewolfToggleSender(int victimIndex){

        object[] data = {_MyIndex, victimIndex};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions{ Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions{Reliability = true};
        PhotonNetwork.RaiseEvent(WerewolfToggle, data, raiseEventOptions, sendOptions);

    }

    public void OnClickRestartBtn(){


        RaiseEventOptions raiseEventOptions = new RaiseEventOptions{ Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions{Reliability = true};
        PhotonNetwork.RaiseEvent(Restart, null, raiseEventOptions, sendOptions);

    }




    #endregion


}
