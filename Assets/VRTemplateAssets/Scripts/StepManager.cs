using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Unity.VRTemplate
{
    // Any procedure UI (CPR, wound care, etc.) implements this
    public interface IResettableUI
    {
        void ResetUI();
    }

    public class StepManager : MonoBehaviour
    {
        [Serializable]
        class Step
        {
            [SerializeField]
            public GameObject stepObject;


            [SerializeField]
            public bool requiresCompletion = false;

            [SerializeField]
            public GameObject continueButton;

            [SerializeField]
            public List<GameObject> objectsToActivate;

            // Generic: drag any component that implements IResettableUI here
            [SerializeField]
            public MonoBehaviour uiToReset;
        }

        [SerializeField]
        public GameObject continueButton;

        [SerializeField]
        List<Step> m_StepList = new List<Step>();

        public int m_CurrentStepIndex = 0;

        [Header("Completion")]
        public UnityEvent onLastStepUnlocked;

        void Start()
        {
            UpdateButtonVisibility();
            ActivateStepObjects(m_CurrentStepIndex);
        }

        public void Next()
        {
            DeactivateStepObjects(m_CurrentStepIndex);
            m_StepList[m_CurrentStepIndex].stepObject.SetActive(false);
            m_CurrentStepIndex = (m_CurrentStepIndex + 1) % m_StepList.Count;
            m_StepList[m_CurrentStepIndex].stepObject.SetActive(true);
            UpdateButtonVisibility();
            ActivateStepObjects(m_CurrentStepIndex);
        }

        public void UnlockContinue()
        {
            Step current = m_StepList[m_CurrentStepIndex];
            if (current.continueButton != null)
                current.continueButton.SetActive(true);

            if (m_CurrentStepIndex == m_StepList.Count - 1)
                onLastStepUnlocked?.Invoke();
        }

        void UpdateButtonVisibility()
        {
            Step current = m_StepList[m_CurrentStepIndex];
            if (current.continueButton == null) return;
            current.continueButton.SetActive(!current.requiresCompletion);
        }

        void ActivateStepObjects(int index)
        {
            Step step = m_StepList[index];

            if (step.objectsToActivate != null)
                foreach (var obj in step.objectsToActivate)
                    if (obj != null) obj.SetActive(true);

            // Reset whichever UI is assigned, if it implements IResettableUI
            if (step.uiToReset is IResettableUI resettable)
                resettable.ResetUI();
        }

        void DeactivateStepObjects(int index)
        {
            Step step = m_StepList[index];
            if (step.objectsToActivate == null) return;
            foreach (var obj in step.objectsToActivate)
                if (obj != null) obj.SetActive(false);
        }
    }
}