////////////////////////////////////////////////////////////////////////////
//
// Copyright 2014 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using SkiaSharp;
using Realms;
using Realms.Sync;
using System.Linq;


namespace DrawXShared
{
    /**
     Class that does almost everything for the demo that can be shared.
     It combines drawing with logging in and connecting to the server.

     DrawXSettingsManager provides a singleton to wrap settings and their local Realm.

    Login
    -----
    Credentials come from DrawXSettings.
    
    */
    public class RealmDraw
    {
        private Realm _realm;
        private Realm.RealmChangedEventHandler _refreshOnRealmUpdate;

        #region DrawingState
        private bool _isDrawing = false;
        private DrawPath _drawPath;
        float _canvasWidth, _canvasHeight;
        const float NORMALISE_TO = 4000.0f;
        #endregion

        #region LoginState
        private bool _waitingForLogin = false;
        #endregion

        #region Settings
        private DrawXSettings Settings => DrawXSettingsManager.Settings;
        private SwatchColor _currentColorCache;
        private SwatchColor currentColor
        {
            get
            {
                if (String.IsNullOrEmpty(_currentColorCache.name))
                {
                    _currentColorCache = SwatchColor.colors[Settings.LastColorUsed];
                }
                return _currentColorCache;
            }
            set
            {
                if (!_currentColorCache.name.Equals(value.name))
                {
                    _currentColorCache = value;
                    DrawXSettingsManager.Write(() => Settings.LastColorUsed = _currentColorCache.name);
                }

            }
        }
        #endregion Settings

        public RealmDraw(float inWidth, float inHeight, Realm.RealmChangedEventHandler refreshOnRealmUpdate)
        {

            if (string.IsNullOrEmpty(Settings.ServerIP))
            {
                // new launch or upgrade from prev version which didn't save credentials

            }

            // TODO close the Realm
            _canvasWidth = inWidth;
            _canvasHeight = inHeight;
            _refreshOnRealmUpdate = refreshOnRealmUpdate;

            // simple local open            
            //_realm = Realm.GetInstance("DrawX.realm");
        }

        internal async void LoginToServerAsync()
        {
            _waitingForLogin = true;
            var s = Settings;
            // TODO allow entering Create User flag on credentials to pass in here instead of false
            var credentials = Credentials.UsernamePassword(s.Username, s.Password, false);
            var user = await User.LoginAsync(credentials, new Uri($"http://{s.ServerIP}"));
            Debug.WriteLine($"Got user logged in with refresh token {user.RefreshToken}");

            var loginConf = new SyncConfiguration(user, new Uri($"realm://{s.ServerIP}/~/Draw"));
            _realm = Realm.GetInstance(loginConf);
            _realm.RealmChanged += _refreshOnRealmUpdate;
            _refreshOnRealmUpdate(_realm, null);  // force initial draw on login
            _waitingForLogin = false;
        }

        private void ScalePointsToStore(ref float w, ref float h)
        {
            w *= NORMALISE_TO / _canvasWidth;
            h *= NORMALISE_TO / _canvasHeight;
        }

        private void ScalePointsToDraw(ref float w, ref float h)
        {
            w *= _canvasWidth / NORMALISE_TO;
            h *= _canvasHeight / NORMALISE_TO;
        }


        public void DrawWBackground(SKCanvas canvas)
        {
            canvas.Clear(SKColors.White);
            var paint = new SKPaint();
            const float w = 56.0f;
            const float h = 167.0f;
            foreach (var swatchName in SwatchColor.colors.Keys)
            {
                Debug.WriteLine($"Loading image {swatchName}");
                var swatchBM = EmbeddedMedia.BitmapNamed(swatchName+".png");
                canvas.DrawBitmap(swatchBM, w, h, paint);
            }
        }


        // replaces the CanvasView.drawRect of the original
        public void DrawTouches(SKCanvas canvas)
        {

            if (_realm == null)
                return;  // too early to have finished login

            // TODO avoid clear and build up new paths incrementally fron the unfinished ones
            DrawWBackground(canvas);

            using (SKPaint paint = new SKPaint())
            {
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = 10;
                paint.IsAntialias = true;
                paint.StrokeCap = SKStrokeCap.Round;
                paint.StrokeJoin = SKStrokeJoin.Round;
                foreach (var drawPath in _realm.All<DrawPath>())
                {
                    using (SKPath path = new SKPath())
                    {
                        var pathColor = SwatchColor.colors[drawPath.color].color;
                        paint.Color = pathColor;
                        bool isFirst = true;
                        foreach (var point in drawPath.points)
                        {
                            // for compatibility with iOS Realm, stores floats, normalised to NORMALISE_TO
                            float fx = (float)point.x;
                            float fy = (float)point.y;
                            ScalePointsToDraw(ref fx, ref fy);
                            if (isFirst)
                            {
                                isFirst = false;
                                path.MoveTo(fx, fy);
                            }
                            else
                            {
                                path.LineTo(fx, fy);
                            }
                        }
                        canvas.DrawPath(path, paint);
                    }
                }
            } // SKPaint
        }


        public void StartDrawing(float inX, float inY)
        {
            if (_realm == null)
            {
                if (!_waitingForLogin)
                    LoginToServerAsync();
                return;  // not yet logged into server, let next touch invoke us
            }
            ScalePointsToStore(ref inX, ref inY);
            _isDrawing = true;
            // TODO smarter guard against _realm null
            _realm.Write(() =>
            {
                _drawPath = new DrawPath() { color = currentColor.name };
                _drawPath.points.Add(new DrawPoint() { x = inX, y = inY });
                _realm.Manage(_drawPath);
            });
        }

        public void AddPoint(float inX, float inY)
        {
            if (_realm == null)
                return;  // not yet logged into server
            if (!_isDrawing)
            {
                // has finished connecting to Realm so this is actually a start
                StartDrawing(inX, inY);
                return;
            }
            ScalePointsToStore(ref inX, ref inY);
            //TODO add check if _drawPath.IsInvalidated
            _realm.Write(() =>
            {
                _drawPath.points.Add(new DrawPoint() { x = inX, y = inY });
            });
        }
        public void StopDrawing(float inX, float inY)
        {
            _isDrawing = false;
            if (_realm == null)
                return;  // not yet logged into server
            ScalePointsToStore(ref inX, ref inY);
            _realm.Write(() =>
            {
                _drawPath.points.Add(new DrawPoint() { x = inX, y = inY });
                _drawPath.drawerID = "";  // TODO work out what the intent is here in original Draw sample!
            });
        }

        public void CancelDrawing()
        {
            _isDrawing = false;
            // TODO wipe current path
        }

        public void ErasePaths()
        {
            _realm.Write(() => _realm.RemoveAll<DrawPath>());
        }
    }
}
