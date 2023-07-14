using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wallet : MonoBehaviour
{
   [SerializeField] float money;

   public event Action OnMoneyChanged;

   public static Wallet i {get; private set;}

   private void Awake()
   {
        i = this;
   }

   public void AddMoney(float amt)
   {
        money += amt;
        OnMoneyChanged?.Invoke();
   }
   public void SubtractMoney(float amt)
   {
        money -= amt;
        OnMoneyChanged?.Invoke();
   }

   public float Money => money;
}
