// �������System�����ռ䣨EventArgs����������
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgress
{
    // 1. ���ȱ���¼���ʵ��IHasProgress�ӿڣ�
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    // 2. ״̬����¼��������¼�������������������
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;

    // ������״̬����Ĳ����ࣨ���ٺͽ��Ȳ�����������
    public class OnStateChangedEventArgs : EventArgs
    {
        public State state;
    }

    // ¯��״̬ö��
    public enum State
    {
        Idle,
        Frying,
        Fried,
        Burned,
    }

    [SerializeField] private FryingRecipeSO[] fryingRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;

    private State state;
    private float fryingTimer;
    private FryingRecipeSO fryingRecipeSO;
    private float burningTimer;
    private BurningRecipeSO burningRecipeSO;

    private void Start()
    {
        state = State.Idle;
    }

    private void Update()
    {
        if (!HasKitchenObject()) return; // ����Ʒʱֱ�ӷ��أ����߼�

        switch (state)
        {
            case State.Idle:
                // ����״̬�����ý�����
                TriggerProgressChanged(0f);
                break;
            case State.Frying:
                fryingTimer += Time.deltaTime;
                // ʵʱ�������ȱ���������޸���������ʵʱ���£�
                TriggerProgressChanged(fryingTimer / fryingRecipeSO.fryingTimerMax);

                if (fryingTimer > fryingRecipeSO.fryingTimerMax)
                {
                    // ��ը��ɣ��滻��Ʒ
                    GetKitchenObject().DestroySelf();
                    KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);

                    Debug.Log("Fried");
                    state = State.Fried;
                    burningTimer = 0f;
                    // ��ֵ��������ȡȼ���䷽ʧ��ʱ���ݴ�
                    burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
                    if (burningRecipeSO == null)
                    {
                        Debug.LogWarning($"[{gameObject.name}] ��Ʒ{GetKitchenObject().GetKitchenObjectSO().name}��ȼ���䷽��");
                        state = State.Idle; // ���䷽ʱ�ص�����
                    }
                    // ����״̬����¼�
                    TriggerStateChanged();
                }
                break;
            case State.Fried:
                if (burningRecipeSO == null) break; // ��ֵ����

                burningTimer += Time.deltaTime;
                // ʵʱ����ȼ�ս��ȱ��
                TriggerProgressChanged(burningTimer / burningRecipeSO.burningTimerMax);

                if (burningTimer > burningRecipeSO.burningTimerMax)
                {
                    // �ս���ɣ��滻��Ʒ
                    GetKitchenObject().DestroySelf();
                    KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);

                    Debug.Log("Burned");
                    state = State.Burned;
                    // �ս������������
                    TriggerProgressChanged(0f);
                    // ����״̬����¼�
                    TriggerStateChanged();
                }
                break;
            case State.Burned:
                // �ս�״̬������������
                TriggerProgressChanged(0f);
                break;
        }
    }

    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            // ¯������Ʒ�����Я����Ʒʱ
            if (player.HasKitchenObject())
            {
                KitchenObjectSO playerObjectSO = player.GetKitchenObject().GetKitchenObjectSO();
                if (HasRecipeWithInput(playerObjectSO))
                {
                    // �������ը����Ʒ
                    player.GetKitchenObject().SetKitchenObjectParent(this);
                    fryingRecipeSO = GetFryingRecipeSOWithInput(playerObjectSO);

                    // �л�����ը״̬
                    state = State.Frying;
                    fryingTimer = 0f;

                    // ����״̬�ͽ��ȱ������ʼ����0��
                    TriggerStateChanged();
                    TriggerProgressChanged(0f);
                }
            }
        }
        else
        {
            // ¯������Ʒ����ҿ���ʱȡ��
            if (!player.HasKitchenObject())
            {
                GetKitchenObject().SetKitchenObjectParent(player);
                state = State.Idle;

                // ����״̬�ͽ��ȱ��������0��
                TriggerStateChanged();
                TriggerProgressChanged(0f);
            }
        }
    }

    // ��װ���������ȱ���¼�����ֵ������
    private void TriggerProgressChanged(float progressNormalized)
    {
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = Mathf.Clamp01(progressNormalized) // ȷ��������0~1֮��
        });
    }

    // ��װ������״̬����¼�����ֵ������
    private void TriggerStateChanged()
    {
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
        {
            state = this.state
        });
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        return GetFryingRecipeSOWithInput(inputKitchenObjectSO) != null;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        return fryingRecipeSO?.output; // ��ֵ��д��
    }

    private FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (FryingRecipeSO recipe in fryingRecipeSOArray)
        {
            if (recipe.input == inputKitchenObjectSO)
            {
                return recipe;
            }
        }
        return null;
    }

    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach (BurningRecipeSO recipe in burningRecipeSOArray)
        {
            if (recipe.input == inputKitchenObjectSO)
            {
                return recipe;
            }
        }
        return null;
    }
}