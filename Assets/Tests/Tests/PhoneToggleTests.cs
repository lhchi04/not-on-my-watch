// using System.Collections;
// using NUnit.Framework;
// using UnityEngine;
// using UnityEngine.TestTools;

// public class PhoneToggleTests
// {
//     private GameObject toggleGO;
//     private PhoneToggle phoneToggle;
//     private GameObject phoneGO;
//     private GameObject attachPointGO;

//     [SetUp]
//     public void SetUp()
//     {
//         phoneGO = new GameObject("Phone");
//         phoneGO.AddComponent<Rigidbody>();

//         attachPointGO = new GameObject("AttachPoint");

//         toggleGO = new GameObject("PhoneToggle");
//         phoneToggle = toggleGO.AddComponent<PhoneToggle>();
//         phoneToggle.phoneObject = phoneGO;
//         phoneToggle.handAttachPoint = attachPointGO.transform;
//         // toggleAction left null — input won't fire but HidePhone/ShowPhone still work
//     }

//     [TearDown]
//     public void TearDown()
//     {
//         Object.Destroy(toggleGO);
//         Object.Destroy(phoneGO);
//         Object.Destroy(attachPointGO);
//     }

//     [UnityTest]
//     public IEnumerator PhoneStartsInactive()
//     {
//         yield return null;
//         Assert.IsFalse(phoneGO.activeSelf, "Phone should be hidden on start");
//     }

//     [UnityTest]
//     public IEnumerator HidePhone_DeactivatesPhoneObject()
//     {
//         phoneGO.SetActive(true);
//         yield return null;

//         phoneToggle.HidePhone();
//         yield return null;

//         Assert.IsFalse(phoneGO.activeSelf);
//     }

//     [UnityTest]
//     public IEnumerator HidePhone_UnparentsPhoneFromHand()
//     {
//         phoneGO.transform.SetParent(attachPointGO.transform);
//         yield return null;

//         phoneToggle.HidePhone();
//         yield return null;

//         Assert.IsNull(phoneGO.transform.parent);
//     }

//     [UnityTest]
//     public IEnumerator HidePhone_RestoresGravityOnRigidbody()
//     {
//         Rigidbody rb = phoneGO.GetComponent<Rigidbody>();
//         rb.isKinematic = true;
//         rb.useGravity = false;
//         yield return null;

//         phoneToggle.HidePhone();
//         yield return null;

//         Assert.IsFalse(rb.isKinematic);
//         Assert.IsTrue(rb.useGravity);
//     }

//     [UnityTest]
//     public IEnumerator HidePhone_WithNullPhone_DoesNotThrow()
//     {
//         phoneToggle.phoneObject = null;
//         yield return null;

//         Assert.DoesNotThrow(() => phoneToggle.HidePhone());
//     }
// }
