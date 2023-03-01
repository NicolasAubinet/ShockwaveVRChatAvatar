using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Contact.Components;

public class AddVRCSenders : Editor
{
    private static void AddVRCContactReceiver(CapsuleCollider collider, bool selfOnly)
    {
        var contactReceiver = collider.gameObject.AddComponent<VRCContactReceiver>();
        contactReceiver.shapeType = ContactBase.ShapeType.Capsule;
        contactReceiver.radius = collider.radius;
        contactReceiver.height = collider.height;
        contactReceiver.position = collider.center;
        Vector3 rotation = Vector3.zero;

        switch (collider.direction)
        {
            case 0: // X
                rotation.z = 90;
                break;
            case 1: // Y
                rotation.y = 90;
                break;
            case 2: // Z
                rotation.x = 90;
                break;
            default:
                Debug.LogError("Could not process capsule collider direction " + collider.direction);
                break;
        }

        contactReceiver.rotation = Quaternion.Euler(rotation);

        contactReceiver.receiverType = ContactReceiver.ReceiverType.Constant;
        contactReceiver.parameter = "Shockwave_";

        contactReceiver.collisionTags.Add("Shockwave"); // Custom
        contactReceiver.collisionTags.Add("Hand");
        contactReceiver.collisionTags.Add("Finger");

        if (selfOnly)
        {
            contactReceiver.allowSelf = true;
            contactReceiver.allowOthers = false;
            contactReceiver.localOnly = false;
        }
        else
        {
            contactReceiver.allowSelf = false;
            contactReceiver.allowOthers = true;
            contactReceiver.localOnly = false;

            contactReceiver.collisionTags.Add("Head");
            contactReceiver.collisionTags.Add("Torso");
            contactReceiver.collisionTags.Add("Foot");
        }
    }

    private static List<T> GetAllComponents<T>(GameObject gameObject)
    {
        List<T> allComponents = new List<T>();
        T[] currentObjComponents = gameObject.GetComponents<T>();
        T[] childrenComponents = gameObject.GetComponentsInChildren<T>();

        allComponents.AddRange(currentObjComponents);
        allComponents.AddRange(childrenComponents);
        return allComponents;
    }

    [MenuItem("GameObject/Shockwave/Convert Capsule Colliders to VRC Contact Receivers", false, -1)]
    public static void AddVRCContactReceivers()
    {
        List<CapsuleCollider> capsuleColliders = GetAllComponents<CapsuleCollider>(Selection.activeGameObject);
        if (capsuleColliders.Count == 0)
        {
            Debug.LogWarning("No capsule colliders found on selected item");
            return;
        }

        foreach (CapsuleCollider collider in capsuleColliders)
        {
            AddVRCContactReceiver(collider, true);
            AddVRCContactReceiver(collider, false);
            DestroyImmediate(collider);
        }

        Debug.Log("Collider to receivers conversion completed successfully");
    }

    [MenuItem("GameObject/Shockwave/Remove self-contact Colliders", false, -1)]
    public static void RemoveSelfVRCContactReceivers()
    {
        List<VRCContactReceiver> receivers = GetAllComponents<VRCContactReceiver>(Selection.activeGameObject);
        if (receivers.Count == 0)
        {
            Debug.LogWarning("No VRCContactReceiver components found on selected item");
            return;
        }

        foreach (VRCContactReceiver receiver in receivers)
        {
            if (receiver.allowSelf)
            {
                DestroyImmediate(receiver);
            }
        }

        Debug.Log("Self colliders removed successfully");
    }
}