#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TogglePanel : MonoBehaviour
{
    [SerializeField] GameObject _VoteTogglePrefab;
    [SerializeField] GameManager GM;
    public List<Toggle> _ToggleList;

    public void InstantiateToggles(List<string> _playerNicknameList, List<bool> _playerAliveList, GameManager.State state)
    {

        foreach (Toggle toggle in _ToggleList)
        {
            Destroy(toggle.gameObject);
        }

        _ToggleList.Clear();


            for(int i = 0; i < _playerAliveList.Count; i++){

                GameObject inst = Instantiate(_VoteTogglePrefab, transform);
                _ToggleList.Add(inst.GetComponent<Toggle>());
                inst.transform.Find("Nickname").GetComponent<Text>().text = _playerNicknameList[i];

                if(state == GameManager.State.Day_Vote)
                    inst.GetComponent<Toggle>().onValueChanged.AddListener((t) => OnToggle4Day(inst.GetComponent<Toggle>(), t));
                else if(state == GameManager.State.Night_Vote)
                    inst.GetComponent<Toggle>().onValueChanged.AddListener((t) => OnToggle4Night(inst.GetComponent<Toggle>(), t));

                if(GM._CurrentState == GameManager.State.Day_Vote){

                    if(_playerAliveList[i] == true){
                        inst.GetComponent<Toggle>().interactable = true;
                    }
                    else{
                        inst.GetComponent<Toggle>().interactable = false;
                    }

                }
                else if(GM._CurrentState == GameManager.State.Night_Vote){

                    if(GM._playerJobList[GM._MyIndex] != GM.MEDIUM){

                        if(GM._playerJobList[GM._MyIndex] == GM.SEER ||
                        GM._playerJobList[GM._MyIndex] != GM.BODYGUARD ||
                        GM._playerJobList[GM._MyIndex] != GM.WEREWOLF ||
                        GM._playerJobList[GM._MyIndex] != GM.POSSESSED){

                            if(_playerAliveList[i] == true){
                                inst.GetComponent<Toggle>().interactable = true;
                            }
                            else{
                                inst.GetComponent<Toggle>().interactable = false;
                            }

                        }


                    }
                    else{

                        if(_playerAliveList[i] == true){
                            inst.GetComponent<Toggle>().interactable = false;
                        }
                        else{
                            inst.GetComponent<Toggle>().interactable = true;
                        }
                        
                    }

                }

                


            }
            Debug.Log(_ToggleList.Count);

    }

    private void OnToggle4Day(Toggle toggle, bool newValue)
    {
        GM.GetComponent<AudioSource>().volume = 0.7f;
        GM.GetComponent<AudioSource>().PlayOneShot(GM._ClickSound);

        if (newValue)
        {
            for (int i = 0; i < _ToggleList.Count; i++)
            {
                if (_ToggleList[i] != toggle)
                    _ToggleList[i].isOn = false;
            }
        }

    }

    private void OnToggle4Night(Toggle toggle, bool newValue){
        int index = -1;
        if(newValue){

            for(int i = 0; i < _ToggleList.Count; i++){
                if(_ToggleList[i] != toggle){
                    _ToggleList[i].isOn = false;
                }
                else{
                    index = i;
                }
            }

        }


        //if werewolf
        if(GM._playerJobList[GM._MyIndex] == GM.WEREWOLF){
            GM.WerewolfToggleSender(index);
        }

    }




    public int WhatsOn_N_FinishVote()
    {

        int index = -1;
        for(int i = 0; i < _ToggleList.Count; i++){

            if(_ToggleList[i].isOn == true && _ToggleList[i].interactable == true){
                index = i;
                break;
            }

        }


        foreach (Toggle toggle in _ToggleList)
        {
            Destroy(toggle.gameObject);
        }

        _ToggleList.Clear();



        return index;
    }





}
