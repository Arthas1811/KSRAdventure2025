using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

//sets handy scene services
public static class HandySceneServices
{
    //checks scene navigation
    public static PhoneSceneNavigation EnsureNavigation(Component owner)
    {
        return EnsureComponent<PhoneSceneNavigation>(owner.gameObject);
    }

    //checks save data manager
    public static SaveDataManager EnsureSaveDataManager()
    {
        SaveDataManager manager = SaveDataManager.Instance;
        if (manager != null)
        {
            return manager;
        }

        manager = UnityEngine.Object.FindFirstObjectByType<SaveDataManager>();
        if (manager != null)
        {
            return manager;
        }

        GameObject managerObject = new GameObject("SaveDataManager");
        return managerObject.AddComponent<SaveDataManager>();
    }

    //checks event system
    public static void EnsureEventSystem()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
        }

        InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
        {
            inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
        }
    }

    public static T EnsureComponent<T>(GameObject owner) where T : Component
    {
        T component = owner.GetComponent<T>();
        return component != null ? component : owner.AddComponent<T>();
    }
}
