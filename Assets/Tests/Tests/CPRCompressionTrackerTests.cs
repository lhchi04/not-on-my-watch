// using System.Collections;
// using NUnit.Framework;
// using UnityEngine;
// using UnityEngine.TestTools;

// public class CPRCompressionTrackerTests
// {
//     private GameObject go;
//     private CPRCompressionTracker tracker;
//     private Transform leftHand;
//     private Transform rightHand;

//     [SetUp]
//     public void SetUp()
//     {
//         go = new GameObject("CPRTracker");
//         tracker = go.AddComponent<CPRCompressionTracker>();

//         leftHand = new GameObject("LeftHand").transform;
//         rightHand = new GameObject("RightHand").transform;

//         leftHand.position = new Vector3(0, 1f, 0);
//         rightHand.position = new Vector3(0, 1f, 0);

//         tracker.leftHand = leftHand;
//         tracker.rightHand = rightHand;
//         tracker.maxCompressionDistance = 0.06f;
//         tracker.greenZoneMin = 0.7f;
//         tracker.greenZoneMax = 1.0f;
//         tracker.releaseThreshold = 0.3f;
//         tracker.targetCompressions = 1;
//     }

//     [TearDown]
//     public void TearDown()
//     {
//         Object.Destroy(go);
//         Object.Destroy(leftHand.gameObject);
//         Object.Destroy(rightHand.gameObject);
//     }

//     [UnityTest]
//     public IEnumerator StartTracking_SetsRestingYFromHandPosition()
//     {
//         leftHand.position = new Vector3(0, 1.2f, 0);
//         rightHand.position = new Vector3(0, 1.2f, 0);

//         tracker.StartTracking();
//         yield return null;

//         // After tracking starts, hands at resting position = 0 depth
//         Assert.AreEqual(0, tracker.CompletedCompressions);
//         Assert.AreEqual(0f, tracker.CurrentDepth, 0.01f);
//     }

//     [UnityTest]
//     public IEnumerator GoodCompression_IncrementsCompletedCount()
//     {
//         tracker.StartTracking();
//         yield return null;

//         // Push hands down into green zone (>= 70% of maxCompressionDistance)
//         float pressDepth = tracker.maxCompressionDistance * 0.85f;
//         leftHand.position -= new Vector3(0, pressDepth, 0);
//         rightHand.position -= new Vector3(0, pressDepth, 0);
//         yield return null;

//         // Release hands back above release threshold
//         leftHand.position = new Vector3(0, 1f, 0);
//         rightHand.position = new Vector3(0, 1f, 0);
//         yield return null;

//         Assert.AreEqual(1, tracker.CompletedCompressions);
//     }

//     [UnityTest]
//     public IEnumerator ShallowCompression_DoesNotCount()
//     {
//         tracker.StartTracking();
//         yield return null;

//         // Push hands only to 20% depth (below greenZoneMin of 70%)
//         float shallowDepth = tracker.maxCompressionDistance * 0.2f;
//         leftHand.position -= new Vector3(0, shallowDepth, 0);
//         rightHand.position -= new Vector3(0, shallowDepth, 0);
//         yield return null;

//         leftHand.position = new Vector3(0, 1f, 0);
//         rightHand.position = new Vector3(0, 1f, 0);
//         yield return null;

//         Assert.AreEqual(0, tracker.CompletedCompressions);
//     }

//     [UnityTest]
//     public IEnumerator OnComplete_FiresWhenTargetReached()
//     {
//         tracker.targetCompressions = 1;
//         bool completed = false;
//         tracker.OnComplete.AddListener(() => completed = true);

//         tracker.StartTracking();
//         yield return null;

//         float pressDepth = tracker.maxCompressionDistance * 0.85f;
//         leftHand.position -= new Vector3(0, pressDepth, 0);
//         rightHand.position -= new Vector3(0, pressDepth, 0);
//         yield return null;

//         leftHand.position = new Vector3(0, 1f, 0);
//         rightHand.position = new Vector3(0, 1f, 0);
//         yield return null;

//         Assert.IsTrue(completed);
//     }

//     [UnityTest]
//     public IEnumerator StopTracking_ResetsDepthToZero()
//     {
//         tracker.StartTracking();
//         yield return null;

//         float pressDepth = tracker.maxCompressionDistance * 0.85f;
//         leftHand.position -= new Vector3(0, pressDepth, 0);
//         rightHand.position -= new Vector3(0, pressDepth, 0);
//         yield return null;

//         tracker.StopTracking();
//         yield return null;

//         Assert.AreEqual(0f, tracker.CurrentDepth);
//     }

//     [UnityTest]
//     public IEnumerator ResetAndStart_ClearsCompressionsAndRestarts()
//     {
//         tracker.targetCompressions = 5;
//         tracker.StartTracking();
//         yield return null;

//         // Do one compression
//         float pressDepth = tracker.maxCompressionDistance * 0.85f;
//         leftHand.position -= new Vector3(0, pressDepth, 0);
//         rightHand.position -= new Vector3(0, pressDepth, 0);
//         yield return null;
//         leftHand.position = new Vector3(0, 1f, 0);
//         rightHand.position = new Vector3(0, 1f, 0);
//         yield return null;

//         Assert.AreEqual(1, tracker.CompletedCompressions);

//         tracker.ResetAndStart();
//         yield return null;

//         Assert.AreEqual(0, tracker.CompletedCompressions);
//     }

//     [UnityTest]
//     public IEnumerator RateFeedback_TooFast_WhenCompressionsBelowInterval()
//     {
//         tracker.targetCompressions = 5;
//         tracker.minRate = 100f;
//         tracker.maxRate = 120f;

//         string lastFeedback = "";
//         tracker.OnRateFeedback.AddListener(msg => lastFeedback = msg);

//         tracker.StartTracking();
//         yield return null;

//         // Do two compressions very quickly to trigger "Too fast"
//         for (int i = 0; i < 2; i++)
//         {
//             float pressDepth = tracker.maxCompressionDistance * 0.85f;
//             leftHand.position -= new Vector3(0, pressDepth, 0);
//             rightHand.position -= new Vector3(0, pressDepth, 0);
//             yield return null;
//             leftHand.position = new Vector3(0, 1f, 0);
//             rightHand.position = new Vector3(0, 1f, 0);
//             yield return null;
//         }

//         // Two compressions in ~2 frames = extremely fast rate => "Too fast"
//         Assert.AreEqual("Too fast", lastFeedback);
//     }

//     [UnityTest]
//     public IEnumerator RateFeedback_TooSlow_WhenCompressionsFarApart()
//     {
//         tracker.targetCompressions = 5;
//         tracker.minRate = 100f;
//         tracker.maxRate = 120f;

//         string lastFeedback = "";
//         tracker.OnRateFeedback.AddListener(msg => lastFeedback = msg);

//         tracker.StartTracking();
//         yield return null;

//         // First compression
//         float pressDepth = tracker.maxCompressionDistance * 0.85f;
//         leftHand.position -= new Vector3(0, pressDepth, 0);
//         rightHand.position -= new Vector3(0, pressDepth, 0);
//         yield return null;
//         leftHand.position = new Vector3(0, 1f, 0);
//         rightHand.position = new Vector3(0, 1f, 0);
//         yield return null;

//         // Wait 2 seconds between compressions (< 100 BPM)
//         yield return new WaitForSeconds(2f);

//         leftHand.position -= new Vector3(0, pressDepth, 0);
//         rightHand.position -= new Vector3(0, pressDepth, 0);
//         yield return null;
//         leftHand.position = new Vector3(0, 1f, 0);
//         rightHand.position = new Vector3(0, 1f, 0);
//         yield return null;

//         Assert.AreEqual("Too slow", lastFeedback);
//     }
// }
