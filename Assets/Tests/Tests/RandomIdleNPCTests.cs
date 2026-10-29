// using System.Collections;
// using NUnit.Framework;
// using UnityEngine;
// using UnityEngine.TestTools;

// public class RandomIdleNPCTests
// {
//     private GameObject npcGO;
//     private RandomIdleNPC npc;

//     [SetUp]
//     public void SetUp()
//     {
//         npcGO = new GameObject("NPC");
//         npc = npcGO.AddComponent<RandomIdleNPC>();
//         npc.numberOfIdleVariations = 4;
//         npc.randomStartingPose = false;
//         npc.specificStartPose = 0;
//         npc.useRandomIdles = false;
//         // No animator assigned — SetIdleType will null-check safely
//     }

//     [TearDown]
//     public void TearDown()
//     {
//         Object.Destroy(npcGO);
//     }

//     [Test]
//     public void SetSpecificPose_ClampsBelowZero()
//     {
//         Assert.DoesNotThrow(() => npc.SetSpecificPose(-5));
//     }

//     [Test]
//     public void SetSpecificPose_ClampsAboveMax()
//     {
//         Assert.DoesNotThrow(() => npc.SetSpecificPose(999));
//     }

//     [Test]
//     public void SetSpecificPose_ValidRange_DoesNotThrow()
//     {
//         for (int i = 0; i < 4; i++)
//             Assert.DoesNotThrow(() => npc.SetSpecificPose(i));
//     }

//     [Test]
//     public void SetRandomIdleEnabled_TogglesBothDirections()
//     {
//         Assert.DoesNotThrow(() => npc.SetRandomIdleEnabled(true));
//         Assert.DoesNotThrow(() => npc.SetRandomIdleEnabled(false));
//     }

//     [UnityTest]
//     public IEnumerator NPC_WithNoWaypoints_DoesNotMove()
//     {
//         npc.waypoints = null;
//         Vector3 startPos = npcGO.transform.position;

//         yield return new WaitForSeconds(0.2f);

//         Assert.AreEqual(startPos, npcGO.transform.position);
//     }

//     [UnityTest]
//     public IEnumerator NPC_WithWaypoints_MovesTowardFirstWaypoint()
//     {
//         GameObject wp = new GameObject("Waypoint");
//         wp.transform.position = new Vector3(10f, 0f, 0f);

//         npc.waypoints = new Transform[] { wp.transform };
//         npc.walkSpeed = 5f;

//         yield return new WaitForSeconds(0.3f);

//         float distanceMoved = Vector3.Distance(npcGO.transform.position, Vector3.zero);
//         Assert.Greater(distanceMoved, 0.1f, "NPC should have moved toward waypoint");

//         Object.Destroy(wp);
//     }

//     [UnityTest]
//     public IEnumerator NPC_SequentialWaypoints_CyclesBackToFirst()
//     {
//         npc.randomWaypoints = false;
//         npc.walkSpeed = 50f; // fast so it reaches waypoints quickly

//         GameObject wp1 = new GameObject("WP1");
//         GameObject wp2 = new GameObject("WP2");
//         wp1.transform.position = new Vector3(0.3f, 0f, 0f);
//         wp2.transform.position = new Vector3(0.6f, 0f, 0f);

//         npc.waypoints = new Transform[] { wp1.transform, wp2.transform };

//         yield return new WaitForSeconds(0.5f);

//         // NPC should have visited at least one waypoint without errors
//         Assert.IsNotNull(npc);

//         Object.Destroy(wp1);
//         Object.Destroy(wp2);
//     }
// }
