// 必须添加System命名空间（EventArgs基类依赖）
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static CuttingCounter;

public class StoveCounter : BaseCounter, IHasProgress
{
    // 1. 进度变更事件（实现IHasProgress接口）
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    // 2. 状态变更事件（独立事件，参数类语义修正）
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;

    // 修正：状态变更的参数类（不再和进度参数类重名）
    public class OnStateChangedEventArgs : EventArgs
    {
        public State state;
    }

    // 炉灶状态枚举
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
        if (!HasKitchenObject()) return; // 无物品时直接返回，简化逻辑

        switch (state)
        {
            case State.Idle:
                // 闲置状态：重置进度条
                TriggerProgressChanged(0f);
                break;
            case State.Frying:
                fryingTimer += Time.deltaTime;
                // 实时触发进度变更（核心修复：进度条实时更新）
                TriggerProgressChanged(fryingTimer / fryingRecipeSO.fryingTimerMax);

                if (fryingTimer > fryingRecipeSO.fryingTimerMax)
                {
                    // 油炸完成：替换物品
                    GetKitchenObject().DestroySelf();
                    KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);

                    Debug.Log("Fried");
                    state = State.Fried;
                    burningTimer = 0f;
                    // 空值保护：获取燃烧配方失败时的容错
                    burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
                    if (burningRecipeSO == null)
                    {
                        Debug.LogWarning($"[{gameObject.name}] 物品{GetKitchenObject().GetKitchenObjectSO().name}无燃烧配方！");
                        state = State.Idle; // 无配方时回到闲置
                    }
                    // 触发状态变更事件
                    TriggerStateChanged();
                }
                break;
            case State.Fried:
                if (burningRecipeSO == null) break; // 空值保护

                burningTimer += Time.deltaTime;
                // 实时触发燃烧进度变更
                TriggerProgressChanged(burningTimer / burningRecipeSO.burningTimerMax);

                if (burningTimer > burningRecipeSO.burningTimerMax)
                {
                    // 烧焦完成：替换物品
                    GetKitchenObject().DestroySelf();
                    KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);

                    Debug.Log("Burned");
                    state = State.Burned;
                    // 烧焦后进度条归零
                    TriggerProgressChanged(0f);
                    // 触发状态变更事件
                    TriggerStateChanged();
                }
                break;
            case State.Burned:
                // 烧焦状态：进度条归零
                TriggerProgressChanged(0f);
                break;
        }
    }

    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            // 炉灶无物品，玩家携带物品时
            if (player.HasKitchenObject())
            {
                KitchenObjectSO playerObjectSO = player.GetKitchenObject().GetKitchenObjectSO();
                if (HasRecipeWithInput(playerObjectSO))
                {
                    // 放入可油炸的物品
                    player.GetKitchenObject().SetKitchenObjectParent(this);
                    fryingRecipeSO = GetFryingRecipeSOWithInput(playerObjectSO);

                    // 切换到油炸状态
                    state = State.Frying;
                    fryingTimer = 0f;

                    // 触发状态和进度变更（初始进度0）
                    TriggerStateChanged();
                    TriggerProgressChanged(0f);
                }
            }
        }
        else
        {
            // 炉灶有物品，玩家空手时取走
            if (!player.HasKitchenObject())
            {
                GetKitchenObject().SetKitchenObjectParent(player);
                state = State.Idle;

                // 触发状态和进度变更（进度0）
                TriggerStateChanged();
                TriggerProgressChanged(0f);
            }
        }
    }

    // 封装：触发进度变更事件（空值保护）
    private void TriggerProgressChanged(float progressNormalized)
    {
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = Mathf.Clamp01(progressNormalized) // 确保进度在0~1之间
        });
    }

    // 封装：触发状态变更事件（空值保护）
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
        return fryingRecipeSO?.output; // 空值简化写法
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