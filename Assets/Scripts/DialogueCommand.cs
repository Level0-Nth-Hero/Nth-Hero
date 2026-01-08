using System.Collections;
using UnityEngine;

public class DialogueCommand : ICommand
{
    private string _text;
    private float _duration;

    public DialogueCommand(string text, float duration)
    {
        _text = text;
        _duration = duration;
    }

    public IEnumerator Execute()
    {
        // UIManager를 통해 대사 출력
        UIManager.Instance.ShowDialogue(_text, _duration);
        
        // 대사가 떠있는 시간만큼 대기 (이게 끝나야 다음 공격 커맨드가 실행됨)
        yield return new WaitForSeconds(_duration);
    }
}