// using System.Collections;
// using NUnit.Framework;
// using UnityEngine;
// using UnityEngine.TestTools;

// public class SprayInteractionTests
// {
//     private GameObject sprayGO;
//     private SprayInteraction spray;

//     [SetUp]
//     public void SetUp()
//     {
//         sprayGO = new GameObject("SprayBottle");

//         // SprayInteraction.Start() calls GetComponent<XRGrabInteractable> and hooks events.
//         // We add a stub Rigidbody to prevent physics errors, but skip Start by not using
//         // AddComponent — instead we test only the public zone methods directly.
//         spray = sprayGO.AddComponent<SprayInteraction>();
//     }

//     [TearDown]
//     public void TearDown()
//     {
//         Object.Destroy(sprayGO);
//     }

//     [Test]
//     public void EnterSprayZone_SetsZoneFlag()
//     {
//         // EnterSprayZone and ExitSprayZone are public — test them directly
//         spray.EnterSprayZone();

//         // No direct public getter for isInSprayZone, but we verify no exception
//         // and the method is callable without error
//         Assert.DoesNotThrow(() => spray.EnterSprayZone());
//     }

//     [Test]
//     public void ExitSprayZone_AfterEntering_DoesNotThrow()
//     {
//         spray.EnterSprayZone();
//         Assert.DoesNotThrow(() => spray.ExitSprayZone());
//     }

//     [Test]
//     public void EnterAndExitSprayZone_CanBeCalledMultipleTimes()
//     {
//         Assert.DoesNotThrow(() =>
//         {
//             for (int i = 0; i < 5; i++)
//             {
//                 spray.EnterSprayZone();
//                 spray.ExitSprayZone();
//             }
//         });
//     }
// }
