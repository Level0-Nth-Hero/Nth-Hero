using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    private Queue<ICommand> commandQueue = new Queue<ICommand>();

    private bool isBusy = false;

    void Awake() {Instance = this;}
    
    public void AddCommand(ICommand command)
    {
        commandQueue.Enqueue(command);
        if (!isBusy)
        {
            StartCoroutine(ProcessCommandRoutine());
        }
    }
    
    IEnumerator ProcessCommandRoutine()
    {
        isBusy = true;
        while (commandQueue.Count > 0)
        {
            ICommand currentCommand = commandQueue.Dequeue();
            yield return StartCoroutine(currentCommand.Execute());
        }
        isBusy = false;
    }
}
