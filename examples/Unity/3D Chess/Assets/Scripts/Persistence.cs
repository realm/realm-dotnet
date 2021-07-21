using System;
using System.Threading.Tasks;
using Realms;
using Realms.Sync;
using UnityEngine;

class Persistence
{
    public static SyncConfiguration SyncConfiguration = default;

    public static async Task CreateSyncConfiguration()
    {
        // You can find the app id in your MongoDB Realm app in Atlas.
        var app = App.Create("3d_chess-sjdkk");
        // For this example we can just randomly create a new user when we start
        // the game since all users will access the same game.
        var email = Guid.NewGuid().ToString();
        var password = "password";
        await app.EmailPasswordAuth.RegisterUserAsync(email, password);
        var user = await app.LogInAsync(Credentials.EmailPassword(email, password));
        SyncConfiguration = new SyncConfiguration("3d_chess_partition_key", user);
    }

    public static async Task ResetDatabase()
    {
        //var realm = await Realm.GetInstanceAsync(SyncConfiguration);
        var realm = Realm.GetInstance();

        AddPiece(realm, PieceType.WhiteRook, new Vector3(1, 0.5f, 1));
        AddPiece(realm, PieceType.WhiteKnight, new Vector3(2, 0.5f, 1));
        AddPiece(realm, PieceType.WhiteBishop, new Vector3(3, 0.5f, 1));
        AddPiece(realm, PieceType.WhiteQueen, new Vector3(4, 0.5f, 1));
        AddPiece(realm, PieceType.WhiteKing, new Vector3(5, 0.5f, 1));
        AddPiece(realm, PieceType.WhiteBishop, new Vector3(6, 0.5f, 1));
        AddPiece(realm, PieceType.WhiteKnight, new Vector3(7, 0.5f, 1));
        AddPiece(realm, PieceType.WhiteRook, new Vector3(8, 0.5f, 1));

        AddPiece(realm, PieceType.WhitePawn, new Vector3(1, 0.5f, 2));
        AddPiece(realm, PieceType.WhitePawn, new Vector3(2, 0.5f, 2));
        AddPiece(realm, PieceType.WhitePawn, new Vector3(3, 0.5f, 2));
        AddPiece(realm, PieceType.WhitePawn, new Vector3(4, 0.5f, 2));
        AddPiece(realm, PieceType.WhitePawn, new Vector3(5, 0.5f, 2));
        AddPiece(realm, PieceType.WhitePawn, new Vector3(6, 0.5f, 2));
        AddPiece(realm, PieceType.WhitePawn, new Vector3(7, 0.5f, 2));
        AddPiece(realm, PieceType.WhitePawn, new Vector3(8, 0.5f, 2));

        AddPiece(realm, PieceType.BlackPawn, new Vector3(1, 0.5f, 7));
        AddPiece(realm, PieceType.BlackPawn, new Vector3(2, 0.5f, 7));
        AddPiece(realm, PieceType.BlackPawn, new Vector3(3, 0.5f, 7));
        AddPiece(realm, PieceType.BlackPawn, new Vector3(4, 0.5f, 7));
        AddPiece(realm, PieceType.BlackPawn, new Vector3(5, 0.5f, 7));
        AddPiece(realm, PieceType.BlackPawn, new Vector3(6, 0.5f, 7));
        AddPiece(realm, PieceType.BlackPawn, new Vector3(7, 0.5f, 7));
        AddPiece(realm, PieceType.BlackPawn, new Vector3(8, 0.5f, 7));

        AddPiece(realm, PieceType.BlackRook, new Vector3(1, 0.5f, 8));
        AddPiece(realm, PieceType.BlackKnight, new Vector3(2, 0.5f, 8));
        AddPiece(realm, PieceType.BlackBishop, new Vector3(3, 0.5f, 8));
        AddPiece(realm, PieceType.BlackQueen, new Vector3(4, 0.5f, 8));
        AddPiece(realm, PieceType.BlackKing, new Vector3(5, 0.5f, 8));
        AddPiece(realm, PieceType.BlackBishop, new Vector3(6, 0.5f, 8));
        AddPiece(realm, PieceType.BlackKnight, new Vector3(7, 0.5f, 8));
        AddPiece(realm, PieceType.BlackRook, new Vector3(8, 0.5f, 8));
    }

    private static void AddPiece(Realm realm, PieceType type, Vector3 position)
    {
        var pieceEntity = new PieceEntity(type, position);
        realm.Write(() =>
        {
            realm.Add(pieceEntity);
        });
    }
}
