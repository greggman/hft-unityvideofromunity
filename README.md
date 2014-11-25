HFT-Unity Video From Unity
==========================

This sample show one example of sending video from Unity to the phones.
It does this in the simplest brute force method possible.

A `WebCamTexture` is created. It's contents is read and scaled to some
smaller size. That smaller size is then sent to the phones. It it only
sent around 10fps.

Sending video is problematic at the moment. Ideally we'd use WebRTC
but iOS (as of 8.1) doesn't yet support WebRTC. Video is normally
sent as deltas from the previous frame along with various kinds of
compression and motion info. That's to get the amount of data needed
to a minimum.

In this case though we are sending every pixel every frame. To do
typical video compression would require a real time video compresser
in Unity and a video decompressor in JavaScript. Those might exist
and would be an area to explore.

This sample though just shows sending uncompressed data. Because it's
uncompressed and because the data is so large you'll likely need
to keep it small or lower the frame rate depending on the number
of players that are receiving the video

NOTE: This is just the [hft-unitysimple](http://github.com/greggman/hft-unitysimple)
example with video tacked in. The video is not displayed in Unity. It is
only displayed in controllers.

<img src="screenshot.png" />

Cloning
-------

[If you want to clone this follow the instructions here](https://github.com/greggman/HappyFunTimes/blob/master/docs/unitydocs.md)



