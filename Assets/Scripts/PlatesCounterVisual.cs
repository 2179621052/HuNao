using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatesCounterVisual : MonoBehaviour
{
    [SerializeField] private PlatesCounter platesCounter;
    [SerializeField] private Transform counterTopPoint;
    [SerializeField] private Transform plateVisualPrefab;

    // 对齐图片变量名：plateVisualGameObjectList
    private List<GameObject> plateVisualGameObjectList;


    // 图片里的Awake方法：初始化列表
    private void Awake()
    {
        plateVisualGameObjectList = new List<GameObject>();
    }


    private void Start()
    {
        // 对齐图片：订阅2个事件（生成+移除）
        platesCounter.OnPlateSpawned += PlatesCounter_OnPlateSpawned;
        platesCounter.OnPlateRemoved += PlatesCounter_OnPlateRemoved;
    }


    // 图片里的“移除盘子”方法（对应OnPlateRemoved事件）
    private void PlatesCounter_OnPlateRemoved(object sender, System.EventArgs e)
    {
        // 取最后一个盘子并销毁
        GameObject plateGameObject = plateVisualGameObjectList[plateVisualGameObjectList.Count - 1];
        plateVisualGameObjectList.Remove(plateGameObject);
        Destroy(plateGameObject);
    }


    // 图片里的“生成盘子”方法（对应OnPlateSpawned事件）
    private void PlatesCounter_OnPlateSpawned(object sender, System.EventArgs e)
    {
        // 实例化盘子（父物体为counterTopPoint）
        Transform plateVisualTransform = Instantiate(plateVisualPrefab, counterTopPoint);

        // 图片里的偏移逻辑（Y轴叠放，0.1f间隔）
        float plateOffsetY = 0.1f;
        plateVisualTransform.localPosition = new Vector3(
            0,
            plateOffsetY * plateVisualGameObjectList.Count,
            0
        );

        // 记录生成的盘子到列表
        plateVisualGameObjectList.Add(plateVisualTransform.gameObject);
    }


    // 取消事件订阅（避免内存泄漏）
    private void OnDestroy()
    {
        platesCounter.OnPlateSpawned -= PlatesCounter_OnPlateSpawned;
        platesCounter.OnPlateRemoved -= PlatesCounter_OnPlateRemoved;
    }
}