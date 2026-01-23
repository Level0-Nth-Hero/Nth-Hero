using System.Collections;
using UnityEngine;

public class DialogueCommand : ICommand // 대사 커맨드
{
    private string _text; // 출력할 대사
    private float _duration; // 대사 표시 시간

    public DialogueCommand(string text, float duration) // 생성자
    {
        _text = text; // 대사 설정
        _duration = duration; // 시간 설정
    }

    public IEnumerator Execute() // 커맨드 실행
    {
        // UIManager를 통해 대사 출력
        UIManager.Instance.ShowDialogue(_text, _duration);
        
        // 대사가 떠있는 시간만큼 대기 (이게 끝나야 다음 공격 커맨드가 실행됨)
        yield return new WaitForSeconds(_duration);
    }
}