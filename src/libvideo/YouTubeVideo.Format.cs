namespace VideoLibrary
{
    // TODO Add/Fix itags
    public partial class YouTubeVideo
    {
        public int Fps
        {
            get
            {
                switch (FormatCode)
                {
                    case 571:
                    case 402:
                    case 401:
                    case 400:
                    case 399:
                    case 398:
                    case 337:
                    case 336:
                    case 335:
                    case 334:
                    case 333:
                    case 332:
                    case 331:
                    case 330:
                    case 272:
                    case 315:
                    case 308:
                    case 303:
                    case 302:
                    case 305:
                    case 304:
                    case 299:
                    case 298:
                        return 60;
                    case 18:
                    case 22:
                    case 37:
                    case 43:
                    case 59:
                    case 397:
                    case 396:
                    case 395:
                    case 394:
                    case 313:
                    case 271:
                    case 248:
                    case 247:
                    case 244:
                    case 243:
                    case 242:
                    case 278:
                    case 138:
                    case 266:
                    case 264:
                    case 137:
                    case 136:
                    case 135:
                    case 134:
                    case 133:
                    case 160:
                        return 30;
                    default:
                        return -1;
                }
            }
        }

        public bool IsAdaptive => this.AdaptiveKind != AdaptiveKind.None;

        public AdaptiveKind AdaptiveKind
        {
            get
            {
                switch (FormatCode)
                {
                    case 18:
                    case 22:
                    case 37:
                    case 43:
                    case 59:
                    case 133:
                    case 134:
                    case 135:
                    case 136:
                    case 137:
                    case 138:
                    case 160:
                    case 242:
                    case 243:
                    case 244:
                    case 247:
                    case 248:
                    case 264:
                    case 266:
                    case 271:
                    case 272:
                    case 298:
                    case 299:
                    case 302:
                    case 303:
                    case 304:
                    case 305:
                    case 308:
                    case 313:
                    case 315:
                    case 330:
                    case 331:
                    case 332:
                    case 333:
                    case 334:
                    case 335:
                    case 336:
                    case 337:
                    case 394:
                    case 395:
                    case 396:
                    case 397:
                    case 398:
                    case 399:
                    case 400:
                    case 401:
                    case 402:
                    case 571:
                    case 278:
                    case 694:
                    case 695:
                    case 696:
                    case 697:
                    case 698:
                    case 699:
                    case 700:
                    case 701:
                        return AdaptiveKind.Video;
                    case 139:
                    case 140:
                    case 141:
                    case 171:
                    case 172:
                    case 249:
                    case 250:
                    case 251:
                    case 256:
                    case 258:
                    case 327:
                    case 338:
                        return AdaptiveKind.Audio;
                    default:
                        return AdaptiveKind.None;
                }
            }
        }

        public int AudioBitrate
        {
            get
            {
                switch (FormatCode)
                {
                    case 139:
                    case 249:
                    case 250:
                    case 599:
                    case 600:
                        return 48;
                    case 18:
                        return 96;
                    case 37:
                    case 43:
                    case 59:
                    case 140:
                    case 171:
                    case 251:
                        return 128;
                    case 22:
                    case 256:
                        return 192;
                    case 141:
                    case 172:
                    case 327:
                    case 774:
                        return 256;
                    case 258:
                    case 325: 
                    case 328: 
                    case 380:
                        return 384;
                    case 338:
                        return 480;
                    case 773:
                        return 960;
                    default:
                        return -1;
                }
            }
        }

        public int Resolution
        {
            get
            {
                switch (FormatCode)
                {
                    case 394:
                    case 330:
                    case 278:
                    case 160:
                    case 694:
                    case 597:
                    case 598:
                        return 144;
                    case 395:
                    case 331:
                    case 242:
                    case 133:
                    case 695:
                        return 240;
                    case 18:
                    case 43:
                    case 396:
                    case 332:
                    case 243:
                    case 134:
                    case 696:
                    case 167:
                        return 360;
                    case 59:
                    case 397:
                    case 333:
                    case 244:
                    case 135:
                    case 697:
                    case 168:
                        return 480;
                    case 22:
                    case 398:
                    case 334:
                    case 302:
                    case 247:
                    case 298:
                    case 136:
                    case 698:
                    case 169:
                    case 612:
                        return 720;
                    case 37:
                    case 399:
                    case 335:
                    case 303:
                    case 248:
                    case 299:
                    case 137:
                    case 699:
                    case 170:
                    case 216:
                    case 616:
                    case 721:
                        return 1080;
                    case 400:
                    case 336:
                    case 308:
                    case 271:
                    case 304:
                    case 264:
                    case 700:
                        return 1440;
                    case 401:
                    case 337:
                    case 315:
                    case 313:
                    case 305:
                    case 266:
                    case 701:
                        return 2160;
                    case 138:
                    case 272:
                    case 402:
                    case 571:
                    case 702:
                        return 4320;
                    default:
                        return -1;
                }
            }
        }

        public override VideoFormat Format
        {
            get
            {
                switch (FormatCode)
                {
                    case 18:
                    case 22:
                    case 37:
                    case 59:
                    case 133:
                    case 134:
                    case 135:
                    case 136:
                    case 137:
                    case 138:
                    case 160:
                    case 264:
                    case 266:
                    case 298:
                    case 299:
                    case 304:
                    case 305:
                    case 394:
                    case 395:
                    case 396:
                    case 397:
                    case 398:
                    case 399:
                    case 400:
                    case 401:
                    case 402:
                    case 571:
                    case 168:
                    case 169:
                    case 170:
                    case 216:
                    case 278:
                    case 694:
                    case 695:
                    case 696:
                    case 697:
                    case 698:
                    case 699:
                    case 700:
                    case 701:
                    case 702:
                    case 721:
                        return VideoFormat.Mp4;
                    case 43:
                    case 242:
                    case 243:
                    case 244:
                    case 247:
                    case 248:
                    case 271:
                    case 272:
                    case 302:
                    case 303:
                    case 308:
                    case 313:
                    case 598:
                    case 597:
                    case 612:
                    case 616:
                        return VideoFormat.WebM;
                    default:
                        return VideoFormat.Unknown;
                }
            }
        }

        public AudioFormat AudioFormat
        {
            get
            {
                switch (FormatCode)
                {
                    case 18:
                    case 22:
                    case 37:
                    case 59:
                    case 139:
                    case 140:
                    case 141:
                    case 256:
                    case 258:
                    case 327:
                    case 325: 
                    case 328: 
                    case 380: 
                    case 599:
                        return AudioFormat.Aac;
                    case 171:
                    case 172:
                        return AudioFormat.Vorbis;
                    case 43:
                    case 249:
                    case 250:
                    case 251:
                    case 338:
                    case 600:
                    case 773:
                    case 774:
                        return AudioFormat.Opus;
                    default:
                        return AudioFormat.Unknown;
                }
            }
        }
    }
}