using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

// public class ICharacter_AssasinSort : IComparer<AIController>
// {
//     public Vector2Int _Position;
//     public int IdTeam;

//     public int Idx;

//     protected int[] __Arr = new int[0];

//     public enum A_FORMATION
//     {
//         LEFT = 1,
//         MID_LEFT = 2,


//         MIDDLE = 3,

//         MID_RIGHT = 4,
//         RIGHT = 5,

//     }
//     public static int[] Idx_L_FORMATION = new int[] {
//         5 , 6, 4, 3,
//          2, 1, 0,
//     };
//     public static int[] Idx_R_FORMATION = new int[] {
//         4,3,5,6,
//          0,2,1
//     };
//     public static int[] Idx_SUB_L_FORMATION = new int[] {
//         6 , 5, 4, 3,
//          2, 1, 0,
//     };
//     public static int[] Idx_SUB_R_FORMATION = new int[] {
//         3 , 4, 5, 6,
//          0, 2, 1,
//     };

//     public static int[] IdxSPECIAL = new int[] {
//          4,5,3,6,
//          0, 2, 1,
//     };

//     public static int[] LEFT = new int[]
//     {
//         0, 3,4
//     };

//     A_FORMATION formation;


//     protected A_FORMATION Formation(int __Idx)
//     {
//         if (__Idx == 1)
//             return A_FORMATION.MIDDLE;

//         for (int i = 0; i  < LEFT.Length; i++)
//         {
//             if (__Idx == LEFT[i])
//             {
//                 if (__Idx == 0 || __Idx == 4)
//                     return A_FORMATION.LEFT;
//                 else
//                     return A_FORMATION.MID_LEFT;
//             }
//         }


//         if (__Idx == 2 || __Idx == 5)
//             return A_FORMATION.RIGHT;
//         else
//             return A_FORMATION.MID_RIGHT;
//     }
//     // public int Compare(AIController x, AIController y)
//     // {
//     //     if (y.Character.PosY == 0 || y.Character.PosY == ChessBoardHexagon.ROW)
//     //     {
//     //         if (y.Character.PosY == x.Character.PosY)
//     //             return y.Character.PosX.CompareTo(x.Character.PosX);

//     //         return 1;
//     //     }
//     //     if (x.Character.PosY == 0 || x.Character.PosY == ChessBoardHexagon.ROW)
//     //     {
//     //         if (y.Character.PosY == x.Character.PosY)
//     //             return x.Character.PosX.CompareTo(y.Character.PosX);

//     //         return -1;
//     //     }

//     //     //if (y.Character.IsSuperMech())
//     //     //    return 1;

//     //     //if (x.Character.IsSuperMech())
//     //     //    return -1;

//     //     int xS = -1;
//     //     int yS = -1;

//     //     for (int i = 0; i < __Arr.Length; i++)
//     //     {
//     //         if (x.idxFormation == __Arr[i])
//     //             xS = i;

//     //         if (y.idxFormation == __Arr[i])
//     //             yS = i;
//     //     }

//     //     return xS.CompareTo(yS);
//     // }
// }


[DisallowMultipleComponent]
public class ChessPiece : MonoBehaviour
{
    private const float MOVE_TIME = 0.1f;
    private const float JUMP_FORCE = 0.5f;

    public int X, Y;
    public Vector2Int GridPoint
    {
        get { return new Vector2Int(X, Y); }

        set
        {
            X = value.x;
            Y = value.y;
        }
    }

    public Vector3 Position { get { return transform.position; } }

    [SerializeField]
    private GameObject _chessObj;
    public GameObject chessObj
    {
        get { return _chessObj; }
        set
        {
            _chessObj = value;
            //bad merge
            // if (value)
            // {
            //     chessCharacter = _chessObj.GetComponentInChildren<BaseCharacter>();
            //     chessCharacter.PosX = X;
            //     chessCharacter.PosY = Y;

            //     aiController = _chessObj.GetComponent<AIController>();
            // }
            // else
            // {
            //     chessCharacter = null;
            //     aiController = null;
            // }
        }
    }

    private MeshRenderer mesh;
    public MeshRenderer Mesh
    {
        get
        {
            if (mesh == null && gameObject != null)
                mesh = GetComponent<MeshRenderer>();
            return mesh;
        }
    }

    // public BaseCharacter chessCharacter { get; private set; }
    // public AIController aiController { get; private set; }

    public ChessPiece(int posX, int poxY)
    {
        X = posX;
        Y = poxY;
    }

    /// <summary>
    /// Rotates to target pos.
    /// </summary>
    private void RotateToTargetPos(Vector3 dir)
    {
        Quaternion rotation = Quaternion.LookRotation(dir, Vector3.up);
        chessObj.transform.rotation = rotation;
    }

    // public void VisualizeMoveTo(GameDataManager gameManager, Vector2Int from, Vector2Int to, float height = -1f, float jumpDuration = -1f)
    // {
    //     if (chessObj == null)
    //         return;

    //     Vector3 desPos = BoardGeometryHexagon.PointFromGrid(to, gameManager.chessBoard.transform.position);
    //     Vector3 startXZ = new Vector3(chessObj.transform.position.x, 0f, chessObj.transform.position.z);
    //     Vector3 endXZ = new Vector3(desPos.x, 0f, desPos.z);
    //     RotateToTargetPos((endXZ - startXZ).normalized);
        
    //     // chessCharacter.GetComponent<CharacterMovement>().desPos = desPos;
    //     // chessCharacter.GetComponent<CharacterMovement>().isMoving = true;
    //     // chessCharacter.OnStartMove?.Invoke(1f);
    // }
}

