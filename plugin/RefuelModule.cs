using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MissionController
{
    class RefuelModule : PartModule
    {
        private Manager manager
        {
            get { return Manager.instance; }
        }

        Vessel vs = new Vessel();

        private List<ResourceTankList> resourceTankList = new List<ResourceTankList>();
        private void add(ResourceTankList m)
        {
            resourceTankList.Add(m);
        }

        [KSPField(isPersistant = true)]
        private double CurrentRS1 = 0;

        [KSPField(isPersistant = true)]
        private double CurrentRS2 = 0;

        [KSPField(isPersistant = true)]
        private bool orderRS1On = false;

        [KSPField(isPersistant = true)]
        private bool orderRS2On = false;
        
        private double SavedRS1 = 0;
        private double savedRS2 = 0;

        private string rS1Name = "none";
        private string rS2Name = "none";

        private double transferRate = 1;
        private double ResourceCost = 0;
        private bool priceCheck = false;
        private double surcharge = .08;
        private double deliveryCharge = .2;
        private float sMcountdown = 10f;

        public void getResourceCost(string name)
        {
            foreach (ConfigNode rNode in Tools.MCSettings.GetNode("RESOURCECOST").nodes)
            {
                if (rNode.name.Equals(name))
                {
                    ResourceCost = Tools.GetValueDefault(rNode, "cost", 0.0);
                }                
            }
        }
        public void getAllResourcesPart()
        {
            foreach (PartResource pr in part.Resources)
            {
                Debug.LogWarning("Found this Resources In Refuel Module " + pr.resourceName.ToString());
                resourceTankList.Add(new ResourceTankList(pr.resourceName));
            }
        }
    
    
        private void chargePurchasePrice(string resource, double current, double saved)
        {
            double difference = 0;
            double totalAmount = 0;
            double SubTotal1 = 0;
            double SubTotal2 = 0;
            double TotalWithCharges = 0;           
            double maxresource = this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(resource).id).maxAmount;
            current = this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(resource).id).amount;
            if (priceCheck != true) { priceCheck = true; saved = current; }
            this.part.RequestResource(resource, -transferRate);
            FuelPurchase = true;
            if (current == maxresource)
            {
                getResourceCost(resource);
                difference = current - saved;
                totalAmount = difference * ResourceCost;
                SubTotal1 = totalAmount * surcharge;
                SubTotal2 = totalAmount * deliveryCharge;
                TotalWithCharges = totalAmount + SubTotal1 + SubTotal2;
                manager.ModCost((int)totalAmount, "Fuel Cost Purchased " + resource + "" + difference);              
                ScreenMessages.PostScreenMessage("Purchased total amount of " + difference + " " + resource + " for $" + totalAmount + " Surcharge of $" + (int)SubTotal1 + " And Delivery Charge of $" + (int)SubTotal2 + " Total Bill is $" + (int)TotalWithCharges + "Check other cost in Finances LogBook", sMcountdown, 0);
                orderRS2On = false;
                if (orderRS1On != false)
                {
                    orderRS2On = true;
                }
                orderRS1On = false;
                ResourceCost = 0;
                priceCheck = false;
                FuelPurchase = false;
            }
        }

        [KSPField(isPersistant = false, guiActive = true, guiName = "Purchasing Fuel:")]
        private bool FuelPurchase = false;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Refueler Landed:")]
        private bool IsLanded = false;       

        [KSPEvent(guiActive = true, active = true, guiName = "Purchase Fuel")]
        public void OrderLF()
        {
            orderRS1On = true;            
        }               
        public override void OnStart(PartModule.StartState state)
        {
            this.part.force_activate();
            if (HighLogic.LoadedSceneIsFlight)
            {
                getAllResourcesPart();
                ResourceTankList rt1 = resourceTankList[0];
                rS1Name = rt1.resource;
                if (resourceTankList.Count > 1)
                {
                    ResourceTankList rt2 = resourceTankList[1];
                    rS2Name = rt2.resource;
                }
                Debug.LogWarning("rS1Name = " + rS1Name + " rS2Name = " + rS2Name);
            }
            
        }

        public override void OnFixedUpdate()
        {
            if (vs.situation.Equals(Vessel.Situations.LANDED))
            {
                IsLanded = true;
                
            }
            else IsLanded = false;

            if (orderRS1On.Equals(true) && vs.situation.Equals(Vessel.Situations.LANDED))
            {
                if (rS1Name == "none" || rS1Name == "Kethane")
                {
                    if (rS1Name == "Kethane")
                    {
                        ScreenMessages.PostScreenMessage("You cannot purchase Kethane, you have to Mine it Steve!");
                        rS1Name = "none";
                    }
                    else
                    {
                        Debug.LogWarning("No 2nd Resource found skiping Load And Purchase"); orderRS2On = false;
                    }
                    orderRS1On = false;
                }
                else chargePurchasePrice(rS1Name, CurrentRS1, SavedRS1);
            }
            if (orderRS2On.Equals(true) && vs.situation.Equals(Vessel.Situations.LANDED))
            {
                if (rS2Name == "none" || rS2Name == "Kethane")
                {
                    if (rS1Name == "Kethane")
                    {
                        ScreenMessages.PostScreenMessage("You cannot purchase Kethane, you have to Mine it Steve! And yes the 2nd slot does check too!");
                        rS2Name = "none";
                    }
                    else
                    {
                        Debug.LogWarning("No 2nd Resource found skiping Load And Purchase"); orderRS2On = false;
                    }
                    orderRS2On = false;
                }
                else { chargePurchasePrice(rS2Name, CurrentRS2, savedRS2); }   
            }

        }
    }

    public class ResourcePurchase : PartModule
    {
        private Manager manager
        {
            get { return Manager.instance; }
        }

        Vessel vs2 = new Vessel();
        private List<ResourceTankList> resourceTankList2 = new List<ResourceTankList>();
        private void add(ResourceTankList m)
        {
            resourceTankList2.Add(m);
        }

        [KSPField(isPersistant = true)]
        public bool purchaseButton = false;
        public bool purchaseButton2 = false;

        public string purchaseRName = "none";
        public string purchaseRName2 = "none";        
        
        public double purchasePrice = 0;

        public double transferRate = 2;
        public bool hasFuel = false;
        public double current = 0;
        public double SavedAmount = 0;
        public float sMcountdown = 10f;

        RefuelModule rf = new RefuelModule();

        public void checkName()
        {
            foreach (PartResource pr in part.Resources)
            {
                Debug.LogWarning("Found this Resources In Purchase Module " + pr.resourceName.ToString());
                resourceTankList2.Add(new ResourceTankList(pr.resourceName));
            }
        }

        public void checkPrice(string name)
        {
            foreach (ConfigNode rNode in Tools.MCSettings.GetNode("RESOURCECOST").nodes)
            {
                if (rNode.name.Equals(name))
                {
                    purchasePrice = Tools.GetValueDefault(rNode, "cost", 0.0);
                }
            }
        }

        public void buyResource(string name)
        {                        
            current = this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(name).id).amount;
            if (current > 0 && hasFuel != true) { hasFuel = true; SavedAmount = current; }            
            this.part.RequestResource(name, transferRate);
            if (current == 0 && hasFuel != false)
            {
                checkPrice(name);
                int totalCost = (int)SavedAmount * (int)purchasePrice;
                manager.modReward(totalCost, "Sold " + name + " for $" + totalCost);
                ScreenMessages.PostScreenMessage("Sold " + name + " for $" + totalCost + " Info Saved to Other Payments in your Finances LogBook",sMcountdown,0);
                purchaseButton2 = false;
                if (purchaseButton != false)
                {
                    purchaseButton2 = true;
                }
                SavedAmount = 0;
                hasFuel = false;
                purchaseButton = false;               
            }
        }      

        public override void OnStart(PartModule.StartState state)
        {
            this.part.force_activate();
            if (HighLogic.LoadedSceneIsFlight)
            {
                checkName();
                ResourceTankList rt0 = resourceTankList2[0];
                purchaseRName = rt0.resource;
                if (resourceTankList2.Count > 1)
                {
                    ResourceTankList rt1 = resourceTankList2[1];
                    purchaseRName2 = rt1.resource;
                }
                Debug.LogWarning("purchaseRName = " + purchaseRName);
            }
        }

        [KSPField(isPersistant = false, guiActive = true, guiName = "Selling Fuel:")]
        private bool FuelSell = false;        

        [KSPEvent(guiActive = true, active = true, guiName = "Sell Current Fuel")]
        public void SellFuel()
        {
            purchaseButton = true;
        }

        public override void OnFixedUpdate()
        {
            
            if (purchaseButton.Equals(true))
            {
                buyResource(purchaseRName);
            }
            if (purchaseButton2.Equals(true))
            {
                if (purchaseRName2 == "none") { Debug.LogWarning("No 2nd Resource found For Sale Skipping"); purchaseButton2 = false; }
                else { buyResource(purchaseRName2); }
            }              
            


        }
        
    }
    public class ResourceTankList
    {
        public string resource;

        public ResourceTankList()
        {
        }
        public ResourceTankList(string resourceName)
        {
            this.resource = resourceName;
        }
    }
}
