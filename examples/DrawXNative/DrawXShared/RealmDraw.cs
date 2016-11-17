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
using System.Collections.Generic;
using System.Diagnostics;
using SkiaSharp;
using Realms;
using Realms.Sync;


namespace DrawXShared
{
    /**
     Class that does almost everything for the demo that can be shared.
     It combines drawing with logging in and connecting to the server.

     DrawXSettingsManager provides a singleton to wrap settings and their local Realm.

    Login
    -----
    Credentials come from DrawXSettings.
    It is the responsibility of external GUI classes to get credentials entered and delay
    starting RealmDraw until connection is made.

    Drawing
    -------
    There are three components to drawing:
    - the background images for "controls"
    - previously drawn, and cached, paths
    - the currently growing paths (one per app updating the shared Realm).
    
    */
    public class RealmDraw
    {
        private Realm _realm;
        private Realm.RealmChangedEventHandler _refreshOnRealmUpdate;
        internal Action CredentialsEditor {get;set;}

        #region DrawingState
        private bool _isDrawing = false;
        private bool _ignoringTouches = false;
        private DrawPath _drawPath;
        private float _canvasWidth, _canvasHeight;
        private const float NORMALISE_TO = 4000.0f;
        private const float PENCIL_MARGIN = 4.0f;
        #endregion

        #region Touch Areas
        private SKRect _loginIconRect;
        private SKRect _loginIconTouchRect;
        // setup in DrawBackground
        private float _pencilWidth;
        private float _pencilsTop;
        private int _numPencils;
        private List<SKBitmap> _pencilBitmaps;
        private SKBitmap _loginIconBitmap;
        #endregion

        #region LoginState
        private bool _waitingForLogin = false;
        #endregion

        #region Settings
        private DrawXSettings Settings => DrawXSettingsManager.Settings;
        private int _currentColorIndex;  // for quick check if pencil we draw is current color
        private SwatchColor _currentColor;
        private SwatchColor currentColor
        {
            get
            {
                if (String.IsNullOrEmpty(_currentColor.Name))
                {
                    _currentColor = SwatchColor.ColorsByName[Settings.LastColorUsed];
                    _currentColorIndex = SwatchColor.Colors.IndexOf(_currentColor);
                }
                return _currentColor;
            }
            set
            {
                if (!_currentColor.Name.Equals(value.Name))
                {
                    _currentColor = value;
                    DrawXSettingsManager.Write(() => Settings.LastColorUsed = _currentColor.Name);
                    _currentColorIndex = SwatchColor.Colors.IndexOf(_currentColor);
                }

            }
        }
        #endregion Settings

        public RealmDraw(float inWidth, float inHeight, Realm.RealmChangedEventHandler refreshOnRealmUpdate)
        {
            // TODO close the Realm
            _canvasWidth = inWidth;
            _canvasHeight = inHeight;
            _refreshOnRealmUpdate = refreshOnRealmUpdate;

            // simple local open            
            //_realm = Realm.GetInstance("DrawX.realm");

            _pencilBitmaps = new List<SKBitmap>(SwatchColor.Colors.Count);
            foreach (var swatch in SwatchColor.Colors)
            {
                _pencilBitmaps.Add( EmbeddedMedia.BitmapNamed(swatch.Name + ".png") );
            }
            _loginIconBitmap = EmbeddedMedia.BitmapNamed("CloudIcon.png");
        }

        internal async void LoginToServerAsync()
        {
            if (_realm != null)
            {
                // TODO more logout?
                _realm.RealmChanged -= _refreshOnRealmUpdate;  // don't want old event notifications from unused Realm
            }
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


        private bool TouchInControlArea(float inX, float inY)
        {
            if (_loginIconTouchRect.Contains(inX, inY))
            {
                CredentialsEditor();
                return true;
            }
            if (inY < _pencilsTop)
                return false;
            int pencilIndex = (int)(inX / (_pencilWidth + PENCIL_MARGIN));
            // see opposite calc in DrawBackground
            var selectecColor = SwatchColor.Colors[pencilIndex];
            if (!selectecColor.Name.Equals(currentColor.Name))
            {
                currentColor = selectecColor;  // will update saved settings
            }
            return true;  // if in this area even if didn't actually change
        }


        private void DrawWBackground(SKCanvas canvas, SKPaint paint)
        {
            canvas.Clear(SKColors.White);
            DrawPencils(canvas, paint);
            DrawLoginIcon(canvas, paint);
        }


        private void DrawPencils(SKCanvas canvas, SKPaint paint)
        {
            // draw pencils, assigning the fields used for touch detection
            _numPencils = SwatchColor.ColorsByName.Count;
            var marginAlloc = (_numPencils + 1) * PENCIL_MARGIN;
            _pencilWidth = (canvas.ClipBounds.Width - marginAlloc) / _numPencils;  // see opposite calc in TouchInControlArea
            var pencilHeight = _pencilWidth * 334.0f / 112.0f;  // scale as per originals
            float runningLeft = PENCIL_MARGIN;
            float pencilsBottom = canvas.ClipBounds.Height;
            _pencilsTop = pencilsBottom - pencilHeight;
            int _pencilIndex = 0;
            foreach (var swatchBM in _pencilBitmaps)
            {
                var pencilRect = new SKRect(runningLeft, _pencilsTop, runningLeft + _pencilWidth, pencilsBottom);
                if (_pencilIndex++ == _currentColorIndex)
                {
                    var offsetY = -Math.Max(20.0f, pencilHeight / 4.0f);
                    pencilRect.Offset(0.0f, offsetY);  // show selected color
                }
                canvas.DrawBitmap(swatchBM, pencilRect, paint);
                runningLeft += PENCIL_MARGIN + _pencilWidth;
            }
        }


        private void DrawLoginIcon(SKCanvas canvas, SKPaint paint)
        {
            if (_loginIconRect.Width <= 0.1f)
            {
                const float ICON_WIDTH = 84.0f;
                const float ICON_HEIGHT = 54.0f;
                #if __IOS__
                const float TOP_BAR_OFFSET = 48.0f;
                #else
                const float TOP_BAR_OFFSET = 8.0f;
                #endif
                _loginIconRect = new SKRect(8.0f, TOP_BAR_OFFSET, 8.0f + ICON_WIDTH, TOP_BAR_OFFSET + ICON_HEIGHT);
                _loginIconTouchRect = new SKRect(0.0f, 0.0f,
                                                 Math.Max(_loginIconRect.Right + 4.0f, 44.0f),
                                                 Math.Max(_loginIconRect.Bottom + 4.0f, 44.0f)
                                                );
            }
            canvas.DrawBitmap(_loginIconBitmap, _loginIconRect, paint);
        }


        // replaces the CanvasView.drawRect of the original
        public void DrawTouches(SKCanvas canvas)
        {

            if (_realm == null)
                return;  // too early to have finished login

            // TODO avoid clear and build up new paths incrementally fron the unfinished ones

            using (SKPaint paint = new SKPaint())
            {
                DrawWBackground(canvas, paint);
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = 10;
                paint.IsAntialias = true;
                paint.StrokeCap = SKStrokeCap.Round;
                paint.StrokeJoin = SKStrokeJoin.Round;
                foreach (var drawPath in _realm.All<DrawPath>())
                {
                    using (SKPath path = new SKPath())
                    {
                        var pathColor = SwatchColor.ColorsByName[drawPath.color].Color;
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
            if (TouchInControlArea(inX, inY))
            {
                _ignoringTouches = true;
                return;
            }
            _ignoringTouches = false;
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
                _drawPath = new DrawPath() { color = currentColor.Name };  // Realm saves name of color
                _drawPath.points.Add(new DrawPoint() { x = inX, y = inY });
                _realm.Manage(_drawPath);
            });
        }

        public void AddPoint(float inX, float inY)
        {
            if (_ignoringTouches)
                return;  // probably touched in pencil area
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
            if (_ignoringTouches)
                return;  // probably touched in pencil area
            _ignoringTouches = false;
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
            _ignoringTouches = false;
            // TODO wipe current path
        }

        public void ErasePaths()
        {
            _realm.Write(() => _realm.RemoveAll<DrawPath>());
        }
    }
}
