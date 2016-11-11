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
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DrawXShared
{
    public class RealmDraw
    {
        private bool _isDrawing = false;
        private Realm _realm;
        private DrawPath _drawPath;

        #region Settings
        private Realm _realmLocalSettings;
        private DrawXSettings _savedSettings;
        private SwatchColor _currentColorCache;
        private SwatchColor currentColor
        {
            get
            {
                if (_savedSettings == null)
                {
                    _savedSettings = _realmLocalSettings.All<DrawXSettings>().FirstOrDefault();
                    if (_savedSettings == null)
                    {
                        _realmLocalSettings.Write(() =>
                        {
                            _savedSettings = _realmLocalSettings.CreateObject<DrawXSettings>();
                            _savedSettings.LastColorUsed = SwatchColor.Indigo.name;
                        });
                    }
                    _currentColorCache = SwatchColor.colors[_savedSettings.LastColorUsed];
                }
                return _currentColorCache;
            }
            set
            {
                if (!_currentColorCache.name.Equals(value.name))
                {
                    _currentColorCache = value;
                    _realmLocalSettings.Write(() => _savedSettings.LastColorUsed = _currentColorCache.name);
                }

            }
        }
        #endregion Settings


        public RealmDraw()
        {
            // TODO close the Realm
            // TODO allow entering credentials
            //Login().RunSynchronously();
            // simple local open            
            _realm = Realm.GetInstance("DrawX.realm");
            var settingsConf = new RealmConfiguration("DrawXsettings.realm");
            settingsConf.ObjectClasses = new[] { typeof(DrawXSettings) };
            settingsConf.SchemaVersion = 1;  // set explicitly and bump as we add setting properties
            _realmLocalSettings = Realm.GetInstance(settingsConf);
        }

        private async Task Login()
        {
            const string ADMIN_TOKEN = "ewoJImlkZW50aXR5IjogImFkbWluIiwKCSJhY2Nlc3MiOiBbInVwbG9hZCIsICJkb3dubG9hZCIsICJtYW5hZ2UiXQp9Cg==:C5cvsDDd43JhqNRTr83zp+w3ivo8PKa7dvPe2qxMRj7YB2ARjJzKwccVjyvYoYK9e9DQPAbXf/38lYeaGSKDrUQKnpAEXDS1KpVFTCRNIoihCil9WhwGlkHaSAAattn4yrD20Ej5mPDHPMvXcbEepotbg5bQYsteiBs5Ehudbw3Stl5NW9jwKhdNxtOAOoEp/iwIHHCOAkQDBEJCnjf9tI0OIoWhMknPgmBLgYUBiX0pogmtr2fzLvK/O84z+hObdzqf39YC2JniIkZnzBpn645zBKJP0Q0aJ3YaL+IEOzsupUhc2+opq/LtxdD9UgoTEd2w8miXFh3A/G2wS4wMbw==";
            var credentials = Credentials.AccessToken(ADMIN_TOKEN, Guid.NewGuid().ToString(), true);
            var user = await User.LoginAsync(credentials, "http://localhost:9080");
            _realm = Realm.GetInstance(new SyncConfiguration(user, new Uri("realm://localhost:9080/drawx")));
        }

        // replaces the CanvasView.drawRect of the original
        public void DrawTouches(SKCanvas canvas, int width, int height)
        {
            // TODO avoid clear and build up new paths incrementally fron the unfinished ones
            canvas.Clear(SKColors.White);

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
                            // for compatibility with iOS Realm, stores doubles
                            float fx = (float)point.x;
                            float fy = (float)point.y;
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


        public void StartDrawing(double inX, double inY)
        {
            _isDrawing = true;
            _realm.Write(() =>
            {
                _drawPath = new DrawPath() { color = currentColor.name };
                _drawPath.points.Add(new DrawPoint() { x = inX, y = inY });
                _realm.Manage(_drawPath);
            });
        }

        public void AddPoint(double inX, double inY)
        {
            Debug.Assert(_isDrawing = true);
            //TODO add check if _drawPath.IsInvalidated
            _realm.Write(() =>
            {
                _drawPath.points.Add(new DrawPoint() { x = inX, y = inY });
            });
        }
        public void StopDrawing(double inX, double inY)
        {
            _realm.Write(() =>
            {
                _drawPath.points.Add(new DrawPoint() { x = inX, y = inY });
                _drawPath.drawerID = "";  // TODO work out what the intent is here in original Draw sample!
            });
            _isDrawing = false;
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
