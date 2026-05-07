using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IMiniGame
{
    void StartGame(Action onSuccess, Action onFail);

    void EndGame();
}
