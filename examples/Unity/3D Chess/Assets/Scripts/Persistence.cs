using System.Linq;
using Realms;
using UnityEngine;

class Persistence
{
    public static IQueryable<PieceEntity> ResetDatabase()
    {
        var realm = Realm.GetInstance();

        realm.Write(() =>
        {
            realm.RemoveAll<PieceEntity>();
        });

        CreatePieceEntity(realm, PieceType.WhiteRook, new Vector3(1, 0, 1));
        CreatePieceEntity(realm, PieceType.WhiteKnight, new Vector3(2, 0, 1));
        CreatePieceEntity(realm, PieceType.WhiteBishop, new Vector3(3, 0, 1));
        CreatePieceEntity(realm, PieceType.WhiteQueen, new Vector3(4, 0, 1));
        CreatePieceEntity(realm, PieceType.WhiteKing, new Vector3(5, 0, 1));
        CreatePieceEntity(realm, PieceType.WhiteBishop, new Vector3(6, 0, 1));
        CreatePieceEntity(realm, PieceType.WhiteKnight, new Vector3(7, 0, 1));
        CreatePieceEntity(realm, PieceType.WhiteRook, new Vector3(8, 0, 1));

        CreatePieceEntity(realm, PieceType.WhitePawn, new Vector3(1, 0, 2));
        CreatePieceEntity(realm, PieceType.WhitePawn, new Vector3(2, 0, 2));
        CreatePieceEntity(realm, PieceType.WhitePawn, new Vector3(3, 0, 2));
        CreatePieceEntity(realm, PieceType.WhitePawn, new Vector3(4, 0, 2));
        CreatePieceEntity(realm, PieceType.WhitePawn, new Vector3(5, 0, 2));
        CreatePieceEntity(realm, PieceType.WhitePawn, new Vector3(6, 0, 2));
        CreatePieceEntity(realm, PieceType.WhitePawn, new Vector3(7, 0, 2));
        CreatePieceEntity(realm, PieceType.WhitePawn, new Vector3(8, 0, 2));

        CreatePieceEntity(realm, PieceType.BlackPawn, new Vector3(1, 0, 7));
        CreatePieceEntity(realm, PieceType.BlackPawn, new Vector3(2, 0, 7));
        CreatePieceEntity(realm, PieceType.BlackPawn, new Vector3(3, 0, 7));
        CreatePieceEntity(realm, PieceType.BlackPawn, new Vector3(4, 0, 7));
        CreatePieceEntity(realm, PieceType.BlackPawn, new Vector3(5, 0, 7));
        CreatePieceEntity(realm, PieceType.BlackPawn, new Vector3(6, 0, 7));
        CreatePieceEntity(realm, PieceType.BlackPawn, new Vector3(7, 0, 7));
        CreatePieceEntity(realm, PieceType.BlackPawn, new Vector3(8, 0, 7));

        CreatePieceEntity(realm, PieceType.BlackRook, new Vector3(1, 0, 8));
        CreatePieceEntity(realm, PieceType.BlackKnight, new Vector3(2, 0, 8));
        CreatePieceEntity(realm, PieceType.BlackBishop, new Vector3(3, 0, 8));
        CreatePieceEntity(realm, PieceType.BlackQueen, new Vector3(4, 0, 8));
        CreatePieceEntity(realm, PieceType.BlackKing, new Vector3(5, 0, 8));
        CreatePieceEntity(realm, PieceType.BlackBishop, new Vector3(6, 0, 8));
        CreatePieceEntity(realm, PieceType.BlackKnight, new Vector3(7, 0, 8));
        CreatePieceEntity(realm, PieceType.BlackRook, new Vector3(8, 0, 8));

        return realm.All<PieceEntity>();
    }

    private static PieceEntity CreatePieceEntity(Realm realm, PieceType type, Vector3 position)
    {
        var pieceEntity = new PieceEntity(type, position);
        realm.Write(() =>
        {
            realm.Add(pieceEntity);
        });

        return pieceEntity;
    }
}
