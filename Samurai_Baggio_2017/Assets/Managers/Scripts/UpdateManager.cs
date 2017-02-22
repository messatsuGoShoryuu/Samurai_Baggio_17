using UnityEngine;

/// <summary>
/// Made by Feiko Joosten
/// 
/// I have based this code on this blogpost. Decided to give it more functionality. http://blogs.unity3d.com/2015/12/23/1k-update-calls/
/// Use this to speed up your performance when you have a lot of update calls in your scene
/// Let the object you want to give increased performance inherit from CustomBehaviour
/// Replace your void Update() for public abstract void UpdateMe()
/// CustomBehaviour will add the object to the update manager
/// UpdateManager will handle all of the update calls
/// 
/// Changed by Sinan Erez: made small changes to CustomBehaviour (automatic unregistering on OnDestroy)
/// </summary>

public class UpdateManager : MonoBehaviour
{
	private static UpdateManager instance;

	private int count = 0;
	private CustomBehaviour[] array;

	public UpdateManager()
	{
		instance = this;
	}

	public static void register(CustomBehaviour behaviour)
	{
		instance.AddItemToArray(behaviour);
	}

	public static void unregister(CustomBehaviour behaviour)
	{
		instance.RemoveSpecificItemFromArray(behaviour);
	}

	public static void RemoveSpecificItemAndDestroyIt(CustomBehaviour behaviour)
	{
		instance.RemoveSpecificItemFromArray(behaviour);

		Destroy(behaviour.gameObject);
	}

	private void AddItemToArray(CustomBehaviour behaviour)
	{
		if(array == null)
		{
			array = new CustomBehaviour[1];
		}
		else
		{
			System.Array.Resize(ref array, array.Length + 1);
		}
		array[array.Length - 1] = behaviour;
		count = array.Length;
	}

	private void RemoveSpecificItemFromArray(CustomBehaviour behaviour)
	{
		int addAt = 0;
		CustomBehaviour[] tempArray = new CustomBehaviour[array.Length - 1];

		for(int i = 0; i < array.Length; i++)
		{
			if(array[i] == null)
			{
				continue;
			}
			else if(array[i] == behaviour)
			{
				continue;
			}
			tempArray[addAt] = array[i];
			addAt++;
		}

		array = new CustomBehaviour[tempArray.Length];

		for (int i = 0; i < tempArray.Length; i++)
		{
			array[i] = tempArray[i];
		}

		count = array.Length;
	}

	private void Update()
	{
		if (count > 0)
		{
			for (var i = 0; i < array.Length; i++)
			{
				if (array[i] == null)
				{
					continue;
				}
				array[i].update();
			}
		}
	}

    private void FixedUpdate()
    {
        if (count > 0)
        {
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                {
                    continue;
                }
                array[i].fixedUpdate();
            }
        }
    }

    private void LateUpdate()
    {
        if (count > 0)
        {
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                {
                    continue;
                }
                array[i].lateUpdate();
            }
        }
    }
}











