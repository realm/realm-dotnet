////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
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
using System.Linq;
using System.Linq.Expressions;
using Realms;
using Realms.Sync;
using SkiaSharp;

namespace DrawXShared
{
    /***
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
    - previously drawn completed, paths which will no longer grow,
    - the currently growing paths (one per app updating the shared Realm).

    However, with the paths, if we only worry about drawing _added_ or _changed_ paths
    then we don't care if they are completed or not, just what more we have to draw of them.

    Caching and Responsiveness
    --------------------------
    Ideally, we want to have almost all the drawn content cached in a bitmap for redisplay, 
    and only draw new line segments for each added point. It is relatively easy to optimise for
    local draw updates because we know when touched that we are drawing a single added segment.

    There are two distinct patterns which can cause a poor demo with synchronised Draw.
    - A long, winding path being updated - there may be enough lag that we have to add more than 
      one point to it locally but we don't want to redraw the whole thing from scratch.      
    - Many small, single "dab" strokes being drawn, say from someone tapping a display, 
      which mean we have, at least, a TouchesBegan and TouchesEnded and probably AddPoint in between.
    
    We make use of a non-persistent field to help a given Draw differentiate which 
    points it has processed - in the case of an incoming path it may have multiple points 
    we have not yet seen.

    Most importantly, to get the fastest possible response as the user moves their finger,
    we draw the local line immediately as a continuation of the path they started drawing 
    earlier. (See _currentlyDrawing)
    
    */
    public class RealmDraw
    {
        private const float NORMALISE_TO = 4000.0f;
        private const float PENCIL_MARGIN = 4.0f;
        private const float INVALID_LAST_COORD = -1.0f;
        private float _lastX = INVALID_LAST_COORD;
        private float _lastY = INVALID_LAST_COORD;

        #region Synchronised data
        private Realm _realm;
        private IQueryable<DrawPath> _allPaths;  // we observe all and filter based on changes
        #endregion

        #region GUI Callbacks
        internal Action RefreshOnRealmUpdate { get; set; }

        internal Action CredentialsEditor { get; set; }
        #endregion

        #region DrawingState
        private bool _isDrawing = false;
        private bool _ignoringTouches = false;
        private DrawPath _drawPath;
        private SKPath _currentlyDrawing;  // caches for responsive drawing on this device
        private float _canvasWidth, _canvasHeight;
        private IList<DrawPath> _pathsToDraw = null;  // set in notification callback
        #endregion

        #region CachedCanvas
        private int _canvasSaveCount;  // from SaveLayer
        private bool _hasSavedBitmap = false;  // separate flag so we don't rely on any given value in _canvasSaveCount
        private bool _redrawPathsAtNextDraw = true;
        #endregion

        #region Touch Areas
        private SKRect _loginIconRect;
        private SKRect _loginIconTouchRect;
        //// setup in DrawBackground
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

        private int _currentColorIndex = -1;  //// for quick check if pencil we draw is current color

        private SwatchColor _currentColor;

        private SwatchColor CurrentColor
        {
            get
            {
                if (string.IsNullOrEmpty(_currentColor.Name))
                {
                    InitCurrentColor();
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

        private void InitCurrentColor()
        {
            _currentColor = SwatchColor.ColorsByName [Settings.LastColorUsed];
            _currentColorIndex = SwatchColor.Colors.IndexOf(_currentColor);
        }
        #endregion Settings

        public RealmDraw(float inWidth, float inHeight)
        {
            // TODO close the Realm
            _canvasWidth = inWidth;
            _canvasHeight = inHeight;

            // simple local open            
            // _realm = Realm.GetInstance("DrawX.realm");

            _pencilBitmaps = new List<SKBitmap>(SwatchColor.Colors.Count);
            foreach (var swatch in SwatchColor.Colors)
            {
                _pencilBitmaps.Add(EmbeddedMedia.BitmapNamed(swatch.Name + ".png"));
            }

            _loginIconBitmap = EmbeddedMedia.BitmapNamed("CloudIcon.png");
        }

        internal void InvalidateCachedPaths()
        {
            _redrawPathsAtNextDraw = true;
            _hasSavedBitmap = false;
            _currentlyDrawing = null;
        }

        internal async void LoginToServerAsync()
        {
            // in case have lingering subscriptions, clear by clearing the results to which we subscribe
            _allPaths = null;

            _waitingForLogin = true;
            var s = Settings;
            //// TODO allow entering Create User flag on credentials to pass in here instead of false
            var credentials = Credentials.UsernamePassword(s.Username, s.Password, false);
            var user = await User.LoginAsync(credentials, new Uri($"http://{s.ServerIP}"));
            Debug.WriteLine($"Got user logged in with refresh token {user.RefreshToken}");

            var loginConf = new SyncConfiguration(user, new Uri($"realm://{s.ServerIP}/~/Draw"));
            _realm = Realm.GetInstance(loginConf);
            _allPaths = _realm.All<DrawPath>() as IQueryable<DrawPath>;
            _allPaths.SubscribeForNotifications((sender, changes, error) =>
            {
                // WARNING ChangeSet indices are only valid inside this callback
                if (changes == null)  // initial call
                {
                    RefreshOnRealmUpdate();  // force initial draw on login
                    return;
                }
                //// we assume if at least one path deleted, drastic stuff happened, probably erase all
                if (_allPaths.Count() == 0 || changes.DeletedIndices.Length > 0)
                {
                    Debug.WriteLine($"Realm notifier: Invalidating on Realm change as paths deleted or none");
                    InvalidateCachedPaths();  // someone erased their tablet
                    RefreshOnRealmUpdate();
                    return;
                }

                var numInserted = changes.InsertedIndices.Length;
                var numChanged = changes.ModifiedIndices.Length;
                Debug.WriteLine($"Realm notifier: {numInserted} inserts, {numChanged} changes");
                if ((numInserted == 0 && numChanged == 1 && _allPaths.ElementAt(changes.ModifiedIndices[0]) == _drawPath) ||
                    (numInserted == 1 && numChanged == 0 && _allPaths.ElementAt(changes.InsertedIndices[0]) == _drawPath))
                {
                    // current path is drawn by immediate action, not by a callback
                    Debug.WriteLine("Realm notifier: no action because is just current path");
                    return;
                }

                _pathsToDraw = new List<DrawPath>();
                foreach (var index in changes.InsertedIndices)
                {
                    Debug.WriteLine($"Realm notifier: caching path object inserted at {index}");
                    _pathsToDraw.Add(_allPaths.ElementAt(index));
                }

                foreach (var index in changes.ModifiedIndices)
                {
                    Debug.WriteLine($"Realm notifier: caching object modified at {index}");
                    _pathsToDraw.Add(_allPaths.ElementAt(index));
                }

                RefreshOnRealmUpdate();
            });
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
                InvalidateCachedPaths();
                CredentialsEditor.Invoke();  // TODO only invalidate if changed server??
                return true;
            }

            if (inY < _pencilsTop)
            {
                return false;
            }

            // see opposite calc in DrawBackground
            var pencilIndex = (int)(inX / (_pencilWidth + PENCIL_MARGIN));
            var selectecColor = SwatchColor.Colors[pencilIndex];
            if (!selectecColor.Name.Equals(CurrentColor.Name))
            {
                CurrentColor = selectecColor;  // will update saved settings
            }

            InvalidateCachedPaths();
            return true;  // if in this area even if didn't actually change
        }

        private void DrawPencils(SKCanvas canvas, SKPaint paint)
        {
            // draw pencils, assigning the fields used for touch detection
            _numPencils = SwatchColor.ColorsByName.Count;
            if (_currentColorIndex == -1) 
            {
                InitCurrentColor();
            }

            var marginAlloc = (_numPencils + 1) * PENCIL_MARGIN;
            _pencilWidth = (canvas.ClipBounds.Width - marginAlloc) / _numPencils;  // see opposite calc in TouchInControlArea
            var pencilHeight = _pencilWidth * 334.0f / 112.0f;  // scale as per originals
            var runningLeft = PENCIL_MARGIN;
            var pencilsBottom = canvas.ClipBounds.Height;
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
                                                 Math.Max(_loginIconRect.Bottom + 4.0f, 44.0f));
            }

            canvas.DrawBitmap(_loginIconBitmap, _loginIconRect, paint);
        }

        private void DrawAPath(SKCanvas canvas, SKPaint paint, DrawPath drawPath)
        {
            using (var path = new SKPath())
            {
                var pathColor = SwatchColor.ColorsByName[drawPath.color].Color;
                paint.Color = pathColor;
                var isFirst = true;
                foreach (var point in drawPath.points)
                {
                    // for compatibility with iOS Realm, stores floats, normalised to NORMALISE_TO
                    var fx = (float)point.x;
                    var fy = (float)point.y;
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

        private void InitPaint(SKPaint paint)
        {
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 10;  // TODO scale width depending on device width
            paint.IsAntialias = true;
            paint.StrokeCap = SKStrokeCap.Round;
            paint.StrokeJoin = SKStrokeJoin.Round;
        }

        // replaces the CanvasView.drawRect of the original
        // draw routine called when screen refreshed
        // Note the canvas only exists during this call
        public void DrawTouches(SKCanvas canvas)
        {
            if (_realm == null)
            {
                return;  // too early to have finished login
            }

            if (_hasSavedBitmap)
            {
                Debug.WriteLine($"DrawTouches - blitting saved canvas");
                canvas.RestoreToCount(_canvasSaveCount);  // use up the offscreen bitmap regardless
            }

            using (var paint = new SKPaint())
            {
                InitPaint(paint);
                if (_redrawPathsAtNextDraw)
                {
                    Debug.WriteLine($"DrawTouches - Redrawing all paths");
                    canvas.Clear(SKColors.White);
                    DrawPencils(canvas, paint);
                    DrawLoginIcon(canvas, paint);
                    foreach (var drawPath in _realm.All<DrawPath>())
                    {
                        DrawAPath(canvas, paint, drawPath);
                    }

                    _pathsToDraw = null;
                }
                else
                {
                    // current paths being drawn, by other devices
                    if (_pathsToDraw != null)
                    {
                        Debug.WriteLine($"DrawTouches - drawing remote paths in progress");
                        foreach (var drawPath in _pathsToDraw)
                        {
                            Debug.WriteLine($"DrawTouches - drawing path from Realm starting at {drawPath.points[0]}");
                            DrawAPath(canvas, paint, drawPath);
                        }
                    }
                }

                if (_currentlyDrawing != null)
                {
                    Debug.WriteLine($"DrawTouches - drawing current in-memory path");
                    paint.Color = CurrentColor.Color;
                    canvas.DrawPath(_currentlyDrawing, paint);
                }

                _canvasSaveCount = canvas.SaveLayer(paint);  // cache everything to-date
                _hasSavedBitmap = true;
            } // SKPaint

            _redrawPathsAtNextDraw = false;
        }

        public void StartDrawing(float inX, float inY)
        {
            _currentlyDrawing = null;  // don't clear in Stop as will lose last point, clear when we know done
            if (TouchInControlArea(inX, inY))
            {
                _ignoringTouches = true;
                return;
            }

            _ignoringTouches = false;
            if (_realm == null)
            {
                if (!_waitingForLogin)
                {
                    LoginToServerAsync();
                }

                return;  // not yet logged into server, let next touch invoke us
            }

            _lastX = inX;
            _lastY = inY;
            Debug.WriteLine($"Writing a new path starting at {inX}, {inY}");

            // start a local path for responsive drawing
            _currentlyDrawing = new SKPath();
            _currentlyDrawing.MoveTo(inX, inY);

            ScalePointsToStore(ref inX, ref inY);
            _isDrawing = true;
            _realm.Write(() =>
            {
                _drawPath = new DrawPath { color = CurrentColor.Name };  // Realm saves name of color
                _drawPath.points.Add(new DrawPoint { x = inX, y = inY });
                _realm.Add(_drawPath);
            });
        }

        public void AddPoint(float inX, float inY)
        {
            if (_ignoringTouches)
            {
                return;  // probably touched in pencil area
            }

            if (_realm == null)
            {
                return;  // not yet logged into server
            }

            if (!_isDrawing)
            {
                // has finished connecting to Realm so this is actually a start
                StartDrawing(inX, inY);
                return;
            }

            _lastX = inX;
            _lastY = inY;
            Debug.WriteLine($"Adding a point at {inX}, {inY}");
            _currentlyDrawing.LineTo(inX, inY);

            ScalePointsToStore(ref inX, ref inY);
            _realm.Write(() =>
            {
                _drawPath.points.Add(new DrawPoint { x = inX, y = inY });
            });
        }

        public void StopDrawing(float inX, float inY)
        {
            if (_ignoringTouches)
            {
                return;  // probably touched in pencil area
            }

            _isDrawing = false;
            if (_realm == null)
            {
                return;  // not yet logged into server
            }

            var movedWhilstStopping = (_lastX == inX) && (_lastY == inY);
            _lastX = INVALID_LAST_COORD;
            _lastY = INVALID_LAST_COORD;

            if (movedWhilstStopping)
            {
                _currentlyDrawing.LineTo(inX, inY);
            }

            Debug.WriteLine($"Ending a path at {inX}, {inY}");
            ScalePointsToStore(ref inX, ref inY);
            _realm.Write(() =>
            {
                if (movedWhilstStopping)
                {
                    _drawPath.points.Add(new DrawPoint { x = inX, y = inY });
                }
            
                _drawPath.drawerID = "";  // objc original uses this to detect a "finished" path
            });
        }

        public void CancelDrawing()
        {
            _isDrawing = false;
            _ignoringTouches = false;
            _lastX = INVALID_LAST_COORD;
            _lastY = INVALID_LAST_COORD;
            InvalidateCachedPaths();
            //// TODO wipe current path
        }

        public void ErasePaths()
        {
            InvalidateCachedPaths();
            _realm.Write(() => _realm.RemoveAll<DrawPath>());
        }
    }
}
