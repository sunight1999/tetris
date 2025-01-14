using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private KeyCode _leftKey;
    [SerializeField] private KeyCode _downKey;
    [SerializeField] private KeyCode _rightKey;
    [SerializeField] private KeyCode _rotateKey;
    [SerializeField] private KeyCode _dropKey;

    [SerializeField] private Stage _stage;

    void Update()
    {
        if (GameManager.Instance.GameState != GameState.Playing)
            return;

        if (Input.GetKeyDown(_leftKey))
            _stage.TryMoveBlock(Direction.Left);
        else if (Input.GetKeyDown(_downKey))
            _stage.TryMoveBlock(Direction.Down);
        else if (Input.GetKeyDown(_rightKey))
            _stage.TryMoveBlock(Direction.Right);
        else if (Input.GetKeyDown(_rotateKey))
            _stage.RotateBlock();
        else if (Input.GetKeyDown(_dropKey))
            _stage.DropBlock();
    }
}
