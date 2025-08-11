using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using MissingScriptDetector.Editor;

namespace MissingScriptDetector.Tests
{
    [TestFixture]
    public class MissingScriptServiceTests
    {
        private MissingScriptService _service;
        private const string TestScenePath = "Packages/com.segur.missing-script-detector/Tests/MissingScriptTestScene.unity";

        [SetUp]
        public void Setup()
        {
            _service = new MissingScriptService();

            // Load test scene
            EditorSceneManager.OpenScene(TestScenePath);
        }

        [TearDown]
        public void Teardown()
        {
            // Clean up scene after test
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
        }

        /// <summary>
        /// Test to verify that when a root object is specified, objects with MissingScript can be correctly detected
        /// from that object and all its children
        /// </summary>
        [Test]
        public void FindGameObjectsWithMissingScripts_WithRootObject_ReturnsAllMissingScriptObjects()
        {
            // Arrange
            GameObject rootObject = GameObject.Find("RootObject");
            Assert.That(rootObject, Is.Not.Null, "RootObject not found");

            // Act
            List<GameObject> result = _service.FindGameObjectsWithMissingScripts(rootObject);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.GreaterThan(0), "No objects with MissingScript found");

            // List of expected object names
            string[] expectedObjectNames = {
                "RootObject",
                "ChildObject1",
                "GrandChildObject1",
                "GrandChildObject2",
                "ChildObject2",
                "GrandChildObject3"
            };

            // Verify that all expected objects are included in the result
            foreach (string expectedName in expectedObjectNames)
            {
                Assert.That(result.Any(go => go.name == expectedName),
                    Is.True,
                    $"Object '{expectedName}' is not included in the result");
            }

            // Verify that all objects in the result actually have MissingScript
            foreach (GameObject gameObject in result)
            {
                Assert.That(HasMissingScript(gameObject),
                    Is.True,
                    $"Object '{gameObject.name}' does not have MissingScript");
            }
        }

        /// <summary>
        /// Test to verify that when a child object is specified, objects with MissingScript are detected only from
        /// that object and its children, and parent or sibling objects are not included
        /// </summary>
        [Test]
        public void FindGameObjectsWithMissingScripts_WithChildObject_ReturnsChildAndGrandChildren()
        {
            // Arrange
            GameObject childObject = GameObject.Find("ChildObject1");
            Assert.That(childObject, Is.Not.Null, "ChildObject1 not found");

            // Act
            List<GameObject> result = _service.FindGameObjectsWithMissingScripts(childObject);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3), "Count does not match ChildObject1 and its children");

            // List of expected object names (ChildObject1 and its children only)
            string[] expectedObjectNames = {
                "ChildObject1",
                "GrandChildObject1",
                "GrandChildObject2"
            };

            // Verify that all expected objects are included in the result
            foreach (string expectedName in expectedObjectNames)
            {
                Assert.That(result.Any(go => go.name == expectedName),
                    Is.True,
                    $"Object '{expectedName}' is not included in the result");
            }

            // Verify that RootObject and ChildObject2 are not included
            Assert.That(result.Any(go => go.name == "RootObject"), Is.False, "RootObject is included in the result");
            Assert.That(result.Any(go => go.name == "ChildObject2"), Is.False, "ChildObject2 is included in the result");
        }

        /// <summary>
        /// Test to verify that when a grandchild object is specified, only that object is detected,
        /// and parent or sibling objects are not included
        /// </summary>
        [Test]
        public void FindGameObjectsWithMissingScripts_WithGrandChildObject_ReturnsOnlyGrandChild()
        {
            // Arrange
            GameObject grandChildObject = GameObject.Find("GrandChildObject1");
            Assert.That(grandChildObject, Is.Not.Null, "GrandChildObject1 not found");

            // Act
            List<GameObject> result = _service.FindGameObjectsWithMissingScripts(grandChildObject);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1), "Only GrandChildObject1 should be included in the result");
            Assert.That(result[0].name, Is.EqualTo("GrandChildObject1"));
        }

        /// <summary>
        /// Test to verify that when a normal object without MissingScript is specified,
        /// only that object is detected
        /// </summary>
        [Test]
        public void FindGameObjectsWithMissingScripts_WithNormalObject_ReturnsOnlyNormalObject()
        {
            // Arrange
            GameObject normalObject = GameObject.Find("NormalObject");
            Assert.That(normalObject, Is.Not.Null, "NormalObject not found");

            // Act
            List<GameObject> result = _service.FindGameObjectsWithMissingScripts(normalObject);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1), "Only NormalObject should be included in the result");
            Assert.That(result[0].name, Is.EqualTo("NormalObject"));
        }

        /// <summary>
        /// Test to verify that when a null object is specified, an empty list is returned
        /// </summary>
        [Test]
        public void FindGameObjectsWithMissingScripts_WithNullObject_ReturnsEmptyList()
        {
            // Act
            List<GameObject> result = _service.FindGameObjectsWithMissingScripts(null);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0), "Empty list should be returned for null object");
        }

        /// <summary>
        /// Test to verify that when RemoveMissingScripts is executed on an object with MissingScript,
        /// the MissingScript is correctly removed
        /// </summary>
        [Test]
        public void RemoveMissingScripts_WithObjectHavingMissingScript_RemovesMissingScript()
        {
            // Arrange
            GameObject targetObject = GameObject.Find("RootObject");
            Assert.That(targetObject, Is.Not.Null, "RootObject not found");

            // Verify MissingScript exists before removal
            Assert.That(HasMissingScript(targetObject), Is.True, "Please confirm that MissingScript exists before removal");

            // Act
            _service.RemoveMissingScripts(targetObject);

            // Assert
            Assert.That(HasMissingScript(targetObject), Is.False, "MissingScript was not removed");
        }

        /// <summary>
        /// Test to verify that when RemoveMissingScripts is executed on an object without MissingScript,
        /// nothing changes
        /// </summary>
        [Test]
        public void RemoveMissingScripts_WithObjectWithoutMissingScript_DoesNothing()
        {
            // Arrange
            // Dynamically create an object without MissingScript
            GameObject normalObject = new GameObject("TestNormalObject");

            // Verify MissingScript does not exist before removal
            Assert.That(HasMissingScript(normalObject), Is.False, "Please confirm that newly created object does not have MissingScript");

            // Act
            _service.RemoveMissingScripts(normalObject);

            // Assert
            Assert.That(HasMissingScript(normalObject), Is.False, "Please confirm that the state without MissingScript is maintained");

            // Cleanup
            Object.DestroyImmediate(normalObject);
        }

        /// <summary>
        /// Test to verify that when RemoveMissingScripts is executed on a null object,
        /// no exception is thrown
        /// </summary>
        [Test]
        public void RemoveMissingScripts_WithNullObject_DoesNotThrowException()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _service.RemoveMissingScripts(null), "Exception should not occur for null object");
        }

        /// <summary>
        /// Test to verify that when RemoveAllMissingScripts is executed on multiple objects,
        /// MissingScript is removed from all objects
        /// </summary>
        [Test]
        public void RemoveAllMissingScripts_WithMultipleObjects_RemovesAllMissingScripts()
        {
            // Arrange
            GameObject rootObject = GameObject.Find("RootObject");
            GameObject childObject = GameObject.Find("ChildObject1");
            GameObject grandChildObject = GameObject.Find("GrandChildObject1");

            Assert.That(rootObject, Is.Not.Null, "RootObject not found");
            Assert.That(childObject, Is.Not.Null, "ChildObject1 not found");
            Assert.That(grandChildObject, Is.Not.Null, "GrandChildObject1 not found");

            // Verify MissingScript exists before removal
            Assert.That(HasMissingScript(rootObject), Is.True, "Please confirm that RootObject has MissingScript");
            Assert.That(HasMissingScript(childObject), Is.True, "Please confirm that ChildObject1 has MissingScript");
            Assert.That(HasMissingScript(grandChildObject), Is.True, "Please confirm that GrandChildObject1 has MissingScript");

            var objectsToRemove = new List<GameObject> { rootObject, childObject, grandChildObject };

            // Act
            _service.RemoveAllMissingScripts(objectsToRemove);

            // Assert
            Assert.That(HasMissingScript(rootObject), Is.False, "RootObject's MissingScript was not removed");
            Assert.That(HasMissingScript(childObject), Is.False, "ChildObject1's MissingScript was not removed");
            Assert.That(HasMissingScript(grandChildObject), Is.False, "GrandChildObject1's MissingScript was not removed");
        }

        /// <summary>
        /// Test to verify that when RemoveAllMissingScripts is executed on an empty list,
        /// no exception is thrown
        /// </summary>
        [Test]
        public void RemoveAllMissingScripts_WithEmptyList_DoesNotThrowException()
        {
            // Arrange
            var emptyList = new List<GameObject>();

            // Act & Assert
            Assert.DoesNotThrow(() => _service.RemoveAllMissingScripts(emptyList), "Exception should not occur for empty list");
        }

        /// <summary>
        /// Test to verify that when RemoveAllMissingScripts is executed on a null list,
        /// no exception is thrown
        /// </summary>
        [Test]
        public void RemoveAllMissingScripts_WithNullList_DoesNotThrowException()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _service.RemoveAllMissingScripts(null), "Exception should not occur for null list");
        }

        /// <summary>
        /// Test to verify that when RemoveAllMissingScripts is executed on a list containing objects
        /// with and without MissingScript, only objects with MissingScript have their MissingScript removed,
        /// and objects without MissingScript remain unchanged
        /// </summary>
        [Test]
        public void RemoveAllMissingScripts_WithMixedObjects_RemovesOnlyMissingScripts()
        {
            // Arrange
            GameObject objectWithMissingScript = GameObject.Find("RootObject");
            // Dynamically create an object without MissingScript
            GameObject objectWithoutMissingScript = new GameObject("TestNormalObject");

            Assert.That(objectWithMissingScript, Is.Not.Null, "RootObject not found");
            Assert.That(objectWithoutMissingScript, Is.Not.Null, "Newly created object not found");

            // Verify state before removal
            Assert.That(HasMissingScript(objectWithMissingScript), Is.True, "Please confirm that RootObject has MissingScript");
            Assert.That(HasMissingScript(objectWithoutMissingScript), Is.False, "Please confirm that newly created object does not have MissingScript");

            var mixedList = new List<GameObject> { objectWithMissingScript, objectWithoutMissingScript };

            // Act
            _service.RemoveAllMissingScripts(mixedList);

            // Assert
            Assert.That(HasMissingScript(objectWithMissingScript), Is.False, "MissingScript of object with MissingScript was not removed");
            Assert.That(HasMissingScript(objectWithoutMissingScript), Is.False, "State of object without MissingScript should not be changed");

            // Cleanup
            Object.DestroyImmediate(objectWithoutMissingScript);
        }

        /// <summary>
        /// Checks if the specified GameObject has MissingScript
        /// </summary>
        /// <param name="gameObject">GameObject to check</param>
        /// <returns>True if MissingScript exists</returns>
        private bool HasMissingScript(GameObject gameObject)
        {
            Component[] components = gameObject.GetComponents<Component>();
            return components.Any(component => component == null);
        }
    }
}
