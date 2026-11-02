using System;
using TMPro;
using UnityEngine;
public sealed class LevelManager : MonoBehaviour
{
 public static LevelManager Instance { get; private set; }
 public enum State { WaitingToStart, Running, Won, Failed }
 [SerializeField] private int levelNumber=1;
 [SerializeField, Min(0.1f)] private float targetTimeSeconds=180f;
 [SerializeField, Min(0)] private int targetMoney=500;
 [SerializeField] private LevelStartUI levelStartUI;
 [SerializeField] private TMP_Text remainingTimeText;
 [SerializeField] private TMP_Text targetMoneyText;
 public event Action OnLevelWon;
 public event Action OnLevelFailed;
 public State CurrentState { get; private set; } = State.WaitingToStart;
 public int LevelNumber=>levelNumber; public float TargetTimeSeconds=>targetTimeSeconds; public int TargetMoney=>targetMoney;
 private float remainingTime; private bool initialized;
 private void Awake(){Instance=this;if(targetTimeSeconds<=0f)Debug.LogWarning(name+" Target Time Seconds must be positive.");if(targetMoney<0)Debug.LogWarning(name+" Target Money cannot be negative.");remainingTime=Mathf.Max(0.1f,targetTimeSeconds);MoneyManager.GetOrCreate().ResetMoney();if(levelStartUI!=null)levelStartUI.OnGameStarted+=BeginLevel;if(MoneyManager.Instance!=null)MoneyManager.Instance.OnMoneyChanged+=MoneyChanged;SetHud();}
 private void Start(){if(levelStartUI==null)BeginLevel();}
 private void BeginLevel(){if(initialized)return;initialized=true;CurrentState=State.Running;SetHud();}
 private void Update(){if(CurrentState!=State.Running)return;remainingTime-=Time.deltaTime;SetHud();if(MoneyManager.Instance!=null&&MoneyManager.Instance.CurrentMoney>=targetMoney){Finish(true);return;}if(remainingTime<=0f)Finish(MoneyManager.Instance!=null&&MoneyManager.Instance.CurrentMoney>=targetMoney);}
 private void MoneyChanged(int value){if(CurrentState==State.Running&&value>=targetMoney)Finish(true);SetHud();}
 private void Finish(bool won){if(CurrentState==State.Won||CurrentState==State.Failed)return;CurrentState=won?State.Won:State.Failed;GameInput.GameplayInputEnabled=false;Time.timeScale=0f;SetHud();if(won){Debug.Log("Level "+levelNumber+" won.");OnLevelWon?.Invoke();}else{Debug.Log("Level "+levelNumber+" failed.");OnLevelFailed?.Invoke();}}
 private void SetHud(){if(remainingTimeText!=null){int seconds=Mathf.Max(0,Mathf.CeilToInt(remainingTime));remainingTimeText.text=string.Format("{0:00}:{1:00}",seconds/60,seconds%60);}if(targetMoneyText!=null)targetMoneyText.text="$"+targetMoney;}
 private void OnDestroy(){if(Instance==this)Instance=null;if(levelStartUI!=null)levelStartUI.OnGameStarted-=BeginLevel;if(MoneyManager.Instance!=null)MoneyManager.Instance.OnMoneyChanged-=MoneyChanged;GameInput.GameplayInputEnabled=true;Time.timeScale=1f;}
}