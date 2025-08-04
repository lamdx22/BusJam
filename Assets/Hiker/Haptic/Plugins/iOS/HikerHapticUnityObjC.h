//
//  HikerHapticUnityObjC.h
//  Unity-iPhone
//
//  Created by Hiker Game on 13/2/25.
//

#ifndef HikerHapticUnityObjC_h
#define HikerHapticUnityObjC_h

#import <Foundation/Foundation.h>
#import <CoreHaptics/CoreHaptics.h>

typedef void (*hapticCallback)(int);
@interface HikerHapticUnityObjC : NSObject {

}

+ (HikerHapticUnityObjC*) shared;

- (void) playContinuousHaptic:(float) intensity :(float) sharpness :(float)duration;
- (void) playTransientHaptic:(float) intensity :(float) sharpness;
- (void) playWithDictionaryFromJsonPattern: (NSString*) jsonDict;
- (void) playWIthAHAPFile: (NSString*) fileName;
- (void) playWithAHAPFileFromURLAsString: (NSString*) urlAsString;
- (void) stop;
- (void) stopPatternPlayer;
- (void) updateContinuousHaptic:(float) intensity :(float) sharpness;
+ (BOOL) isSupportHaptic;
+ (void) registerCallbacks:(hapticCallback) patternFinishedCallback :(hapticCallback) engineStoppedCallback;
@end

#endif
