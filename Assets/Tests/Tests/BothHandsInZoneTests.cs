// using System.Collections;
// using NUnit.Framework;
// using UnityEngine;
// using UnityEngine.TestTools;

// public class BothHandsInZoneTests
// {
//     private GameObject zoneGO;
//     private BothHandsInZone zone;

//     [SetUp]
//     public void SetUp()
//     {
//         zoneGO = new GameObject("Zone");
//         BoxCollider col = zoneGO.AddComponent<BoxCollider>();
//         col.isTrigger = true;
//         col.size = new Vector3(2f, 2f, 2f);

//         zone = zoneGO.AddComponent<BothHandsInZone>();
//         zone.requiredTag = "PlayerHand";
//     }

//     [TearDown]
//     public void TearDown()
//     {
//         Object.Destroy(zoneGO);
//     }

//     private GameObject CreateHand(string name, Vector3 pos)
//     {
//         GameObject hand = new GameObject(name);
//         hand.tag = "PlayerHand";
//         SphereCollider col = hand.AddComponent<SphereCollider>();
//         col.radius = 0.1f;
//         Rigidbody rb = hand.AddComponent<Rigidbody>();
//         rb.isKinematic = true;
//         hand.transform.position = pos;
//         return hand;
//     }

//     [UnityTest]
//     public IEnumerator BothHandsEnter_FiresOnBothHandsEntered()
//     {
//         bool eventFired = false;
//         zone.OnBothHandsEntered.AddListener(() => eventFired = true);

//         GameObject hand1 = CreateHand("Hand1", new Vector3(0.3f, 0f, 0f));
//         GameObject hand2 = CreateHand("Hand2", new Vector3(-0.3f, 0f, 0f));

//         // Move hands inside the zone
//         hand1.transform.position = new Vector3(0.3f, 0f, 0f);
//         hand2.transform.position = new Vector3(-0.3f, 0f, 0f);

//         yield return new WaitForFixedUpdate();
//         yield return new WaitForFixedUpdate();

//         Assert.IsTrue(eventFired, "OnBothHandsEntered should fire when both hands are in zone");

//         Object.Destroy(hand1);
//         Object.Destroy(hand2);
//     }

//     [UnityTest]
//     public IEnumerator OneHandEnter_DoesNotFireBothHandsEvent()
//     {
//         bool eventFired = false;
//         zone.OnBothHandsEntered.AddListener(() => eventFired = true);

//         GameObject hand1 = CreateHand("Hand1", new Vector3(0.3f, 0f, 0f));

//         yield return new WaitForFixedUpdate();
//         yield return new WaitForFixedUpdate();

//         Assert.IsFalse(eventFired, "OnBothHandsEntered should not fire with only one hand");

//         Object.Destroy(hand1);
//     }

//     [UnityTest]
//     public IEnumerator UntaggedObject_DoesNotTriggerZone()
//     {
//         bool eventFired = false;
//         zone.OnBothHandsEntered.AddListener(() => eventFired = true);

//         // Create objects without the required tag
//         GameObject obj1 = new GameObject("Obj1");
//         obj1.AddComponent<SphereCollider>().radius = 0.1f;
//         obj1.AddComponent<Rigidbody>().isKinematic = true;
//         obj1.transform.position = new Vector3(0.1f, 0f, 0f);

//         GameObject obj2 = new GameObject("Obj2");
//         obj2.AddComponent<SphereCollider>().radius = 0.1f;
//         obj2.AddComponent<Rigidbody>().isKinematic = true;
//         obj2.transform.position = new Vector3(-0.1f, 0f, 0f);

//         yield return new WaitForFixedUpdate();
//         yield return new WaitForFixedUpdate();

//         Assert.IsFalse(eventFired, "Untagged objects should not trigger the zone");

//         Object.Destroy(obj1);
//         Object.Destroy(obj2);
//     }
// }
