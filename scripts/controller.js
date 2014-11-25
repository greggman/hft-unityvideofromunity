/*
 * Copyright 2014, Gregg Tavares.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are
 * met:
 *
 *     * Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above
 * copyright notice, this list of conditions and the following disclaimer
 * in the documentation and/or other materials provided with the
 * distribution.
 *     * Neither the name of Gregg Tavares. nor the names of its
 * contributors may be used to endorse or promote products derived from
 * this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
 * OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
"use strict";

// Start the main app logic.
requirejs([
    'hft/commonui',
    'hft/gameclient',
    'hft/misc/input',
    'hft/misc/misc',
    'hft/misc/mobilehacks',
    'hft/misc/touch',
  ], function(
    CommonUI,
    GameClient,
    Input,
    Misc,
    MobileHacks,
    Touch) {

  var globals = {
    debug: false,
  };
  Misc.applyUrlSettings(globals);
  MobileHacks.fixHeightHack();

  var score = 0;
  var statusElem = document.getElementById("gamestatus");
  var inputElem = document.getElementById("inputarea");
  var colorElem = document.getElementById("display");
  var videoElem = document.getElementById("video");
  var client = new GameClient();
  var videoCtx = videoElem.getContext("2d");

  // Note: CommonUI handles these events for almost all the samples.
  var onConnect = function() {
    statusElem.innerHTML = "you've connected to happyfuntimes";
  };

  var onDisconnect = function() {
    statusElem.innerHTML = "you were disconnected from happyfuntimes";
  }

  // If I was going to handle this without CommonUI this is what I'd do
  //client.addEventListener('connect', onConnect);
  //client.addEventListener('disconnect', onDisconnect);

  // Because I want the CommonUI to work
  globals.disconnectFn = onDisconnect;
  globals.connectFn = onConnect;

  CommonUI.setupStandardControllerUI(client, globals);

  var randInt = function(range) {
    return Math.floor(Math.random() * range);
  };

  // Sends a move command to the game.
  //
  // This will generate a 'move' event in the corresponding
  // NetPlayer object in the game.
  var sendMoveCmd = function(position, target) {
    client.sendCmd('move', {
      x: position.x / target.clientWidth,
      y: position.y / target.clientHeight,
    });
  };

  // Pick a random color
  var color =  'rgb(' + randInt(256) + "," + randInt(256) + "," + randInt(256) + ")";
  // Send the color to the game.
  //
  // This will generate a 'color' event in the corresponding
  // NetPlayer object in the game.
  client.sendCmd('color', {
    color: color,
  });
  colorElem.style.backgroundColor = color;

  // Send a message to the game when the screen is touched
  inputElem.addEventListener('pointermove', function(event) {
    var position = Input.getRelativeCoordinates(event.target, event);
    sendMoveCmd(position, event.target);
    event.preventDefault();
    CommonUI.log(JSON.stringify(position));
  });

  // Update our score when the game tells us.
  client.addEventListener('scored', function(cmd) {
    score += cmd.points;
    statusElem.innerHTML = "You scored: " + cmd.points + " total: " + score;
  });

  var videoPixels;
  client.addEventListener('video', function(data) {
    // Resize the canvas to match the size of the video's data
    if (videoElem.width != data.width ||
        videoElem.height != data.height) {
      videoElem.width = data.width;
      videoElem.height = data.height;
    }

    // Compute the height we want to display the canvas give the height of the original video
    // Note: The CSS says the canvas's width is 100% of its container
    var width = videoElem.clientWidth;   // The current display width of the canvas
    var height = Math.floor(width * data.origHeight / data.origWidth);
    if (videoElem.clientHeight != height) {
      videoElem.style.height = height + "px";
    }

    // If we haven't allocated videoPixels yet OR if the size doesn't match
    // (re)allocate it.
    if (!videoPixels || videoPixels.width != data.width || videoPixels.height != data.height) {
      videoPixels = videoCtx.createImageData(data.width, data.height);

      // Clear to white (really we only care about setting the alpha to 255)
      var pixels = videoPixels.data;
      var len = data.width * data.height;
      for (var ii = 0; ii < len; ++ii) {
        var offset = ii * 4;
        pixels[offset + 0] = 0xFF;
        pixels[offset + 1] = 0xFF;
        pixels[offset + 2] = 0xFF;
        pixels[offset + 3] = 0xFF;
      }
    }

    // Copy the video data into videoPixels
    var pixels = videoPixels.data;
    for (var yy = 0; yy < data.height; ++yy) {
      for (var xx = 0; xx < data.width; ++xx) {
        var c = data.pixels[yy * data.width + xx];
        var offset = ((data.height - yy - 1) * data.width + xx)  * 4;
        pixels[offset + 0] = (c >> 16) & 0xFF;
        pixels[offset + 1] = (c >>  8) & 0xFF;
        pixels[offset + 2] = (c >>  0) & 0xFF;
      }
    }

    // Draw videoPixels into the canvas.
    videoCtx.putImageData(videoPixels, 0, 0);
  });
});

