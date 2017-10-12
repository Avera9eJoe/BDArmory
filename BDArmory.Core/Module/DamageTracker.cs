﻿using BDArmory.Core.Extension;
using UnityEngine;

namespace BDArmory.Core.Module
{
    public class DamageTracker : PartModule
    {
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Damage"),
        UI_ProgressBar(affectSymCounterparts = UI_Scene.None,controlEnabled = false,scene = UI_Scene.All,maxValue = 100000,minValue = 0,requireFullControl = false)]
        public float Damage = 0f;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Armor"),
        UI_FloatRange(minValue = 15f, maxValue = 1000f, stepIncrement = 5f, scene = UI_Scene.All)]
        public float Armor = 15f;

        //TODO: Add setting
        private readonly float maxDamageFactor = 100f;

        private MaterialColorUpdater damageRenderer;
        private Gradient g = new Gradient();        
    
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            damageRenderer = new MaterialColorUpdater(this.part.transform, PhysicsGlobals.TemperaturePropertyID);

            if (part != null)
            {
                //Add Damage
                UI_ProgressBar damageFieldFlight = (UI_ProgressBar)Fields["Damage"].uiControlFlight;
                damageFieldFlight.maxValue = CalculateMaxDamage();
                damageFieldFlight.minValue = 0f;

                UI_ProgressBar damageFieldEditor = (UI_ProgressBar)Fields["Damage"].uiControlEditor;
                damageFieldEditor.maxValue = CalculateMaxDamage();
                damageFieldEditor.minValue = 0f;
                
                //Add Armor
                UI_FloatRange armorFieldFlight = (UI_FloatRange)Fields["Armor"].uiControlFlight;
                armorFieldFlight.maxValue = 1000f;
                armorFieldFlight.minValue = 15f;

                UI_FloatRange armorFieldEditor = (UI_FloatRange)Fields["Armor"].uiControlEditor;
                armorFieldEditor.maxValue = 1000f;
                armorFieldEditor.minValue = 15f;

                part.RefreshAssociatedWindows();

            }
            else
            {
                Debug.Log("[BDArmory]:DamageTracker::OnStart part  is null");
            }
        }

        public override void OnUpdate()
        {
            //TODO: Add effects
            if (!HighLogic.LoadedSceneIsFlight || !FlightGlobals.ready || this.Damage == 0f)
            {
                return;
            }

            damageRenderer?.Update(GetDamageColor());
        }

        private float CalculateMaxDamage()
        {
            return maxDamageFactor * Mathf.Clamp(part.mass, 0.001f, 50f) * Mathf.Clamp(part.crashTolerance, 1, 25);
        }

        public void DestroyPart()
        {
            part.temperature = part.maxTemp * 2;
        }

        public float GetMaxArmor()
        {
            UI_FloatRange armorField = (UI_FloatRange)Fields["Armor"].uiControlEditor;

            return armorField.maxValue;
        }

        public float GetMaxPartDamage()
        {
            UI_ProgressBar damageField = (UI_ProgressBar) Fields["Damage"].uiControlEditor;

            return damageField.maxValue;
        }        

        public  Color GetDamageColor()
        {
            Color color = PhysicsGlobals.BlackBodyRadiation.Evaluate(Mathf.Clamp01(part.Damage() / part.MaxDamage()));
            color.a *= PhysicsGlobals.BlackBodyRadiationAlphaMult * part.blackBodyRadiationAlphaMult; ;
            return color;
        }

        void OnDestroy()
        {

           

        }

        public void SetDamage(float partdamage)
        {
            Damage = partdamage;
            if (Damage > GetMaxPartDamage())
            {
                DestroyPart();
            }
        }

        public void AddDamage(float partdamage)
        {
            partdamage = Mathf.Max(partdamage, 0.01f);
            Damage += partdamage;
            if (Damage > GetMaxPartDamage())
            {
                DestroyPart();
            }
        }

        public void ReduceArmor(float massToReduce)
        {
            Armor -= massToReduce;
            if (Armor < 0) Armor = 0;
        }
    }
}
