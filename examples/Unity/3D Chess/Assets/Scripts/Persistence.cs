using System.Linq;
using Realms;
using UnityEngine;

class Persistence
{
    public static IQueryable<PieceEntity> ResetDatabase(Realm realm)
    {
        realm.Write(() =>
        {
            realm.RemoveAll<PieceEntity>();

            realm.Add(new PieceEntity(PieceType.WhiteRook, new Vector3(1, 0, 1)));
            realm.Add(new PieceEntity(PieceType.WhiteKnight, new Vector3(2, 0, 1)));
            realm.Add(new PieceEntity(PieceType.WhiteBishop, new Vector3(3, 0, 1)));
            realm.Add(new PieceEntity(PieceType.WhiteQueen, new Vector3(4, 0, 1)));
            realm.Add(new PieceEntity(PieceType.WhiteKing, new Vector3(5, 0, 1)));
            realm.Add(new PieceEntity(PieceType.WhiteBishop, new Vector3(6, 0, 1)));
            realm.Add(new PieceEntity(PieceType.WhiteKnight, new Vector3(7, 0, 1)));
            realm.Add(new PieceEntity(PieceType.WhiteRook, new Vector3(8, 0, 1)));

            realm.Add(new PieceEntity(PieceType.WhitePawn, new Vector3(1, 0, 2)));
            realm.Add(new PieceEntity(PieceType.WhitePawn, new Vector3(2, 0, 2)));
            realm.Add(new PieceEntity(PieceType.WhitePawn, new Vector3(3, 0, 2)));
            realm.Add(new PieceEntity(PieceType.WhitePawn, new Vector3(4, 0, 2)));
            realm.Add(new PieceEntity(PieceType.WhitePawn, new Vector3(5, 0, 2)));
            realm.Add(new PieceEntity(PieceType.WhitePawn, new Vector3(6, 0, 2)));
            realm.Add(new PieceEntity(PieceType.WhitePawn, new Vector3(7, 0, 2)));
            realm.Add(new PieceEntity(PieceType.WhitePawn, new Vector3(8, 0, 2)));

            realm.Add(new PieceEntity(PieceType.BlackPawn, new Vector3(1, 0, 7)));
            realm.Add(new PieceEntity(PieceType.BlackPawn, new Vector3(2, 0, 7)));
            realm.Add(new PieceEntity(PieceType.BlackPawn, new Vector3(3, 0, 7)));
            realm.Add(new PieceEntity(PieceType.BlackPawn, new Vector3(4, 0, 7)));
            realm.Add(new PieceEntity(PieceType.BlackPawn, new Vector3(5, 0, 7)));
            realm.Add(new PieceEntity(PieceType.BlackPawn, new Vector3(6, 0, 7)));
            realm.Add(new PieceEntity(PieceType.BlackPawn, new Vector3(7, 0, 7)));
            realm.Add(new PieceEntity(PieceType.BlackPawn, new Vector3(8, 0, 7)));

            realm.Add(new PieceEntity(PieceType.BlackRook, new Vector3(1, 0, 8)));
            realm.Add(new PieceEntity(PieceType.BlackKnight, new Vector3(2, 0, 8)));
            realm.Add(new PieceEntity(PieceType.BlackBishop, new Vector3(3, 0, 8)));
            realm.Add(new PieceEntity(PieceType.BlackQueen, new Vector3(4, 0, 8)));
            realm.Add(new PieceEntity(PieceType.BlackKing, new Vector3(5, 0, 8)));
            realm.Add(new PieceEntity(PieceType.BlackBishop, new Vector3(6, 0, 8)));
            realm.Add(new PieceEntity(PieceType.BlackKnight, new Vector3(7, 0, 8)));
            realm.Add(new PieceEntity(PieceType.BlackRook, new Vector3(8, 0, 8)));
        });

        return realm.All<PieceEntity>();
    }
}
