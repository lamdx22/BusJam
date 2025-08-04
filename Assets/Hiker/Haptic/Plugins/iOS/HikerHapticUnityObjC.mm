//
//  HikerHapticUnityObjC.mm
//  Unity-iPhone
//
//  Created by Hiker Game on 13/2/25.
//

#import "HikerHapticUnityObjC.h"
@interface HikerHapticUnityObjC()
@property (nonatomic, strong) CHHapticEngine* engine;
@property (nonatomic, strong) id<CHHapticAdvancedPatternPlayer> continuousPlayer;
@property (nonatomic, strong) id<CHHapticPatternPlayer> patternPlayer;
@property (nonatomic) BOOL isEngineStarted;
@property (nonatomic) BOOL isEngineIsStopping;
@property (nonatomic) BOOL isSupportHaptic;
@end

@implementation HikerHapticUnityObjC

static HikerHapticUnityObjC *_shared;
static hapticCallback onHapticPatternFinished = NULL;
static hapticCallback onHapticEngineStopped = NULL;

+ (HikerHapticUnityObjC*) shared {
    @synchronized (self) {
        if (_shared == nil) {
            _shared = [[self alloc] init];
        }
    }
    return _shared;
}

+ (void)registerCallbacks:(hapticCallback)patternFinishedCallback :(hapticCallback)engineStoppedCallback
{
    onHapticEngineStopped = engineStoppedCallback;
    onHapticPatternFinished = patternFinishedCallback;
}

+ (BOOL) isSupportHaptic {
    return self.isSupportHaptic;
}

- (id) init {
    if (self == [super init]) {
        if (@available(iOS 13, *))
        {
            self.isSupportHaptic = CHHapticEngine.capabilitiesForHardware.supportsHaptics;
        }
        else
        {
            self.isSupportHaptic = FALSE;
        }
#if DEBUG
        NSLog(@"[HikerHapticUnityObjC] isSupportHaptic -> %d", self.isSupportHaptic);
#endif
        [self createEngine];
    }
    return self;
}
- (void) dealloc {
#if DEBUG
    NSLog(@"[HikerHapticUnityObjC] dealloc");
#endif
    if (self.isSupportHaptic)
    {
        self.engine = NULL;
        self.continuousPlayer = NULL;
    }
}

- (void) playContinuousHaptic:(float) intensity :(float)sharpness :(float)duration {
  #if DEBUG
      NSLog(@"[HikerHapticUnityObjC] playContinuousHaptic --> intensity: %f, sharpness: %f, isSupportHaptic: %d, engine: %@, player: %@", intensity, sharpness, self.isSupportHaptic, self.engine, self.continuousPlayer);
  #endif

    if (intensity > 1 || intensity <= 0) return;
    if (sharpness > 1 || sharpness < 0) return;
    if (duration <= 0 || duration > 30) return;

    if (self.isSupportHaptic) {

        if (self.engine == NULL) {
            [self createEngine];
        }
        [self startEngine];

        [self createContinuousPlayer:intensity :sharpness :duration];

        NSError* error = nil;
        [_continuousPlayer startAtTime:0 error:&error];

        if (error != nil) {
            NSLog(@"[HikerHapticUnityObjC] Engine play continuous error --> %@", error);
        } else {

        }
    }
}


- (void) playTransientHaptic:(float) intensity :(float)sharpness {
  #if DEBUG
      NSLog(@"[HikerHapticUnityObjC] playTransientHaptic --> intensity: %f, sharpness: %f, isSupportHaptic: %d, engine: %@", intensity, sharpness, self.isSupportHaptic, self.engine);
  #endif

    if (intensity > 1 || intensity <= 0) return;
    if (sharpness > 1 || sharpness < 0) return;

    if (self.isSupportHaptic) {

        if (self.engine == NULL) {
            [self createEngine];
        }
        [self startEngine];

        CHHapticEventParameter* intensityParam = [[CHHapticEventParameter alloc] initWithParameterID:CHHapticEventParameterIDHapticIntensity value:intensity];
        CHHapticEventParameter* sharpnessParam = [[CHHapticEventParameter alloc] initWithParameterID:CHHapticEventParameterIDHapticSharpness value:sharpness];

        CHHapticEvent* event = [[CHHapticEvent alloc] initWithEventType:CHHapticEventTypeHapticTransient parameters:@[intensityParam, sharpnessParam] relativeTime:0];

        NSError* error = nil;
        CHHapticPattern* pattern = [[CHHapticPattern alloc] initWithEvents:@[event] parameters:@[] error:&error];

        if (error == nil) {
            id<CHHapticPatternPlayer> player = [_engine createPlayerWithPattern:pattern error:&error];

            if (error == nil) {
                [player startAtTime:0 error:&error];
            } else {
                NSLog(@"[CoreHapticUnityObjC] Create transient player error --> %@", error);
            }
        } else {
            NSLog(@"[CoreHapticUnityObjC] Create transient pattern error --> %@", error);
        }
    }
}

- (void) playWithDictionaryPattern: (NSDictionary*) hapticDict {
    if (self.isSupportHaptic) {

        if (self.engine == NULL) {
            [self createEngine];
        }
        [self startEngine];

        NSError* error = nil;
        CHHapticPattern* pattern = [[CHHapticPattern alloc] initWithDictionary:hapticDict error:&error];

        if (error == nil) {
            _patternPlayer = [_engine createPlayerWithPattern:pattern error:&error];

            [_engine notifyWhenPlayersFinished:^CHHapticEngineFinishedAction(NSError * _Nullable error) {
                if (error == NULL || error == nil) {
                     if (onHapticPatternFinished != NULL) {
                         onHapticPatternFinished(0);
                     }
                    return CHHapticEngineFinishedActionLeaveEngineRunning;
                } else {
                     if (onHapticPatternFinished != NULL) {
                         onHapticPatternFinished((int)error.code);
                     }
                    return CHHapticEngineFinishedActionStopEngine;
                }
            }];

            if (error == nil) {
                [_patternPlayer startAtTime:0 error:&error];
            } else {
                NSLog(@"[HikerHapticUnityObjC] Create dictionary player error --> %@", error);
            }
        } else {
            NSLog(@"[HikerHapticUnityObjC] Create dictionary pattern error --> %@", error);
        }
    }
}

- (void) playWithDictionaryFromJsonPattern: (NSString*) jsonDict {
    if (jsonDict != nil) {
        #if DEBUG
            NSLog(@"[HikerHapticUnityObjC] playWithDictionaryFromJsonPattern --> json: %@", jsonDict);
        #endif

        NSError* error = nil;
        NSData* data = [jsonDict dataUsingEncoding:NSUTF8StringEncoding];
        NSDictionary* dict = [NSJSONSerialization JSONObjectWithData:data options:NSJSONReadingMutableContainers error:&error];

        if (error == nil) {
            [self playWithDictionaryPattern:dict];
        } else {
            NSLog(@"[HikerHapticUnityObjC] Create dictionary from json error --> %@", error);
        }
    } else {
        NSLog(@"[HikerHapticUnityObjC] Json dictionary string is nil");
    }
}

- (void) playWIthAHAPFile: (NSString*) fileName {
    if (self.isSupportHaptic) {

        if (self.engine == NULL) {
            [self createEngine];
        }
        [self startEngine];

        NSString* path = [[NSBundle mainBundle] pathForResource:fileName ofType:@"ahap"];
        [self playWithAHAPFileFromURLAsString:path];
    }
}

- (void) playWithAHAPFileFromURLAsString: (NSString*) urlAsString {
    if (urlAsString != nil) {
        NSURL* url = [NSURL fileURLWithPath:urlAsString];
        [self playWithAHAPFileFromURL:url];
    } else {
        NSLog(@"[HikerHapticUnityObjC] url string is nil");
    }
}

- (void) playWithAHAPFileFromURL: (NSURL*) url {
    NSError * error = nil;
    [_engine playPatternFromURL:url error:&error];

    if (error != nil) {
        NSLog(@"[HikerHapticUnityObjC] Engine play from AHAP file error --> %@", error);
    }
}

- (void) updateContinuousHaptic:(float) intensity :(float)sharpness {
  #if DEBUG
      NSLog(@"[HikerHapticUnityObjC] updateContinuousHaptic --> intensity: %f, sharpness: %f, isSupportHaptic: %d, engine: %@, player: %@", intensity, sharpness, self.isSupportHaptic, self.engine, self.continuousPlayer);
  #endif

    if (intensity > 1 || intensity <= 0) return;
    if (sharpness > 1 || sharpness < 0) return;

    if (self.isSupportHaptic && _engine != NULL && _continuousPlayer != NULL) {

        CHHapticDynamicParameter* intensityParam = [[CHHapticDynamicParameter alloc] initWithParameterID:CHHapticDynamicParameterIDHapticIntensityControl value:intensity relativeTime:0];
        CHHapticDynamicParameter* sharpnessParam = [[CHHapticDynamicParameter alloc] initWithParameterID:CHHapticDynamicParameterIDHapticSharpnessControl value:sharpness relativeTime:0];

        NSError* error = nil;
        [_continuousPlayer sendParameters:@[intensityParam, sharpnessParam] atTime:0 error:&error];

        if (error != nil) {
            NSLog(@"[HikerHapticUnityObjC] Update contuous parameters error --> %@", error);
        }
    }
}

- (void) stop {
    NSLog(@"[HikerHapticUnityObjC] STOP isSupportHaptic -> %d", self.isSupportHaptic);
    if (self.isSupportHaptic) {

      NSError* error = nil;
      if (_continuousPlayer != NULL)
          [_continuousPlayer stopAtTime:0 error:&error];

      if (_patternPlayer != NULL)
          [_patternPlayer stopAtTime:0 error:&error];

      if (_engine != NULL && _isEngineStarted && !_isEngineIsStopping) {
          __weak HikerHapticUnityObjC *weakSelf = self;

          _isEngineIsStopping = true;
          [_engine stopWithCompletionHandler:^(NSError *error) {
              if (error != nil) {
                NSLog(@"[HikerHapticUnityObjC] The engine stopped with error: %@", error);
              }
              weakSelf.isEngineStarted = false;
              weakSelf.isEngineIsStopping = false;

              if (onHapticEngineStopped != NULL) {
                  onHapticEngineStopped((int)error.code);
              }
          }];
      }
    }
};

- (void) stopPatternPlayer {
    NSLog(@"[HikerHapticUnityObjC] STOP PLAYER isSupportHaptic -> %d, _patternPlayer -> %@", self.isSupportHaptic, _patternPlayer);
    if (self.isSupportHaptic && _patternPlayer != NULL) {
        NSError* error;
        [_patternPlayer stopAtTime:0 error:&error];

        if (error != nil) {
            NSLog(@"[HikerHapticUnityObjC] Player stop error --> %@", error);
        }
    }
}

- (void) createContinuousPlayer {
    [self createContinuousPlayer: 1.0 :0.5 :30];
}

- (void) createContinuousPlayer:(float) intens :(float)sharp :(float) duration {
    if (self.isSupportHaptic) {
        CHHapticEventParameter* intensity = [[CHHapticEventParameter alloc] initWithParameterID:CHHapticEventParameterIDHapticIntensity value:intens];
        CHHapticEventParameter* sharpness = [[CHHapticEventParameter alloc] initWithParameterID:CHHapticEventParameterIDHapticSharpness value:sharp];

        CHHapticEvent* event = [[CHHapticEvent alloc] initWithEventType:CHHapticEventTypeHapticContinuous parameters:@[intensity, sharpness] relativeTime:0 duration:duration];

        NSError* error = nil;
        CHHapticPattern* pattern = [[CHHapticPattern alloc] initWithEvents:@[event] parameters:@[] error:&error];

        if (error == nil) {
            _continuousPlayer = [_engine createAdvancedPlayerWithPattern:pattern error:&error];
        } else {
            NSLog(@"[HikerHapticUnityObjC] Create contuous player error --> %@", error);
        }
    }
}

- (void) createEngine {
    if (self.isSupportHaptic) {
        NSError* error = nil;
        _engine = [[CHHapticEngine alloc] initAndReturnError:&error];

        if (error == nil) {

            _engine.playsHapticsOnly = true;
            __weak HikerHapticUnityObjC *weakSelf = self;

            _engine.stoppedHandler = ^(CHHapticEngineStoppedReason reason) {
                NSLog(@"[HikerHapticUnityObjC] The engine stopped for reason: %ld", (long)reason);
                switch (reason) {
                    case CHHapticEngineStoppedReasonAudioSessionInterrupt:
                        NSLog(@"[HikerHapticUnityObjC] Audio session interrupt");
                        break;
                    case CHHapticEngineStoppedReasonApplicationSuspended:
                        NSLog(@"[HikerHapticUnityObjC] Application suspended");
                        break;
                    case CHHapticEngineStoppedReasonIdleTimeout:
                        NSLog(@"[HikerHapticUnityObjC] Idle timeout");
                        break;
                    case CHHapticEngineStoppedReasonSystemError:
                        NSLog(@"[HikerHapticUnityObjC] System error");
                        break;
                    case CHHapticEngineStoppedReasonNotifyWhenFinished:
                        NSLog(@"[HikerHapticUnityObjC] Playback finished");
                        break;

                    default:
                        NSLog(@"[HikerHapticUnityObjC] Unknown error");
                        break;
                }

                weakSelf.isEngineStarted = false;
            };

            _engine.resetHandler = ^{
                [weakSelf startEngine];
            };
        } else {
            NSLog(@"[HikerHapticUnityObjC] Engine init error --> %@", error);
        }
    }
}

- (void) startEngine {
    if (!_isEngineStarted) {
        NSError* error = nil;
        [_engine startAndReturnError:&error];

        if (error != nil) {
            NSLog(@"[HikerHapticUnityObjC] Engine start error --> %@", error);
        } else {
            _isEngineStarted = true;
        }
    }
}

- (NSString*) createNSString: (const char*) string {
  if (string)
      return [[NSString alloc] initWithUTF8String:string];
  else
      return [NSString stringWithUTF8String: ""];
}
@end

#pragma mark - Bridge

extern "C" {
    void _hikerHapticUnityPlayContinuous(float intensity, float sharpness, float duration) {
        [[HikerHapticUnityObjC shared] playContinuousHaptic:intensity :sharpness :duration];
    }

    void _hikerHapticUnityPlayTransient(float intensity, float sharpness) {
        [[HikerHapticUnityObjC shared] playTransientHaptic:intensity :sharpness];
    }

    void _hikerHapticUnityStop() {
        [[HikerHapticUnityObjC shared] stop];
    }

    void _hikerHapticUnityStopPlayer() {
        [[HikerHapticUnityObjC shared] stopPatternPlayer];
    }

    void _hikerHapticUnityupdateContinuousHaptics(float intensity, float sharpness) {
        [[HikerHapticUnityObjC shared] updateContinuousHaptic:intensity :sharpness];
    }

    void _hikerHapticUnityplayWithDictionaryPattern(const char* jsonDict) {
        [[HikerHapticUnityObjC shared] playWithDictionaryFromJsonPattern:[[HikerHapticUnityObjC shared] createNSString:jsonDict]];
    }

    void _hikerHapticUnityplayWIthAHAPFile(const char* filename) {
        [[HikerHapticUnityObjC shared] playWIthAHAPFile:[[HikerHapticUnityObjC shared] createNSString:filename]];
    }

    void _hikerHapticUnityplayWithAHAPFileFromURLAsString(const char* urlAsString) {
        [[HikerHapticUnityObjC shared] playWithAHAPFileFromURLAsString:[[HikerHapticUnityObjC shared] createNSString:urlAsString]];
    }

    bool _hikerHapticUnityIsSupport() {
        return [[HikerHapticUnityObjC shared] isSupportHaptic];
    }

    void _hikerHapticUnityRegisterCallback(hapticCallback patternFinishedCallback, hapticCallback engineStoppedCallback) {
        [HikerHapticUnityObjC registerCallbacks:patternFinishedCallback :engineStoppedCallback];
    }
}
