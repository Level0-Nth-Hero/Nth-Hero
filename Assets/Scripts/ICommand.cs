using UnityEngine;
using System.Collections;

public interface ICommand // 커맨드 인터페이스
{
    IEnumerator Execute(); // 커맨드 실행 메서드
}
