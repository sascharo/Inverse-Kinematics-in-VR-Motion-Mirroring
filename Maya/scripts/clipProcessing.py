import os, statistics
from collections import OrderedDict
import maya.cmds as cmds

class Clip:
    timeUnits = {'hour':1./3600.,'min':1./60.,'sec':1,'millisec':1000.,
                 'game':15., 'film':24., 'pal':25., 'ntsc':30., 'show':48., 'palf':50., 'ntscf':60.,
                 '23.976fps':23.976, '29.97fps':29.97, '29.97df':29.97, '47.952fps':47.952, '59.94fps':59.94, '44100fps':44100., '48000fps':48000.}
    filterKernels = ['closest', 'lirp', 'box', 'triangle', 'gaussian2', 'gaussian4']

    loco = True
    root = 'Root_01'
    feet = {'l':'Left_Foot_01', 'r':'Right_Foot_01'}
    toes = {'l': 'Left_Toe_01', 'r': 'Right_Toe_01'}
    fpsAttr = 'fps'
    idleAttr = 'idle'
    forwardAttr = 'forward'
    backwardAttr = 'backward'
    locomotionAttr = 'locomotion'
    magnitudeAttr = 'magnitude'
    magnitudeSmoothAttr = 'magnitudeSmooth'
    magnitudeMeanAttr = 'magnitudeMean'
    feetOffsetAttr = 'feetOffset'
    feetOffsetDefaultAttr = 'feetOffsetDefault'
    toesOffsetDefaultAttr = 'toesOffsetDefault'
    feetThresholdAttrs = ['feetThresholdLow', 'feetThresholdHigh']
    toesThresholdAttr = 'toesThreshold'
    footContactAttrs = {'l':'footContactL', 'r':'footContactR'}
    footContactResamplePeriodAttr = 'footContactResamplePeriod'
    footContactReductionAttr = 'footContactReduction'
    footContactResampleKernelAttr = 'footContactResampleKernel'
    footContactShiftAttr = 'footContactShift'
    #zPosDefaultAttr = 'zPosDefault'
    scaleFactorAttr = 'scaleFactor'

    # def lerp(self, val, bl=0., bu=1.):
    #     if val < bl:
    #         return 1.
    #     if val > bu:
    #         return 0.
    #     return (bu - val) / bu

    # def __init__(self):
    #     pass

    def importClip(self, inPath, clip, unityExportSet, outPath):
        fbxFilepath = os.path.join(inPath, f"{clip['i']}.fbx")
        r = cmds.file(fbxFilepath, i=True, type='FBX', iv=True, ra=True, mnc=True, ns=':', op='v=0;', pr=True, ifr=True, itr='override')

        cmds.setAttr(f'{unityExportSet}.unityFbxModelFilePath', outPath, type='string')
        cmds.setAttr(f'{unityExportSet}.unityFbxModelFileName', f"{clip['o']}.proc.fbx", type='string')
        cmds.setAttr(f'{unityExportSet}.unityFbxAnimFilePath', outPath, type='string')
        cmds.setAttr(f'{unityExportSet}.unityFbxAnimFileName', f"{clip['o']}.proc.fbx", type='string')

        self.type = clip['t']

        self.dir = clip['d']

        return r

    def process(self, root, bake=False):
        start = int(cmds.playbackOptions(q=True, min=True))
        end = int(cmds.playbackOptions(q=True, max=True))
        fps = self.timeUnits[cmds.currentUnit(q=True, t=True)]

        cmds.currentTime(start)

        feetOffsetDefault = cmds.getAttr(f"{self.root}.{self.feetOffsetDefaultAttr}")
        toesOffsetDefault = cmds.getAttr(f"{self.root}.{self.toesOffsetDefaultAttr}")
        feetThresholds = [cmds.getAttr(f"{self.root}.{self.feetThresholdAttrs[0]}"),
                          cmds.getAttr(f"{self.root}.{self.feetThresholdAttrs[1]}")]
        toesThreshold = cmds.getAttr(f"{self.root}.{self.toesThresholdAttr}")
        footContactResamplePeriod = cmds.getAttr(f"{self.root}.{self.footContactResamplePeriodAttr}")
        footContactReduction = cmds.getAttr(f"{self.root}.{self.footContactReductionAttr}")
        footContactResampleKernel = cmds.getAttr(f"{self.root}.{self.footContactResampleKernelAttr}")
        footContactShift = cmds.getAttr(f"{self.root}.{self.footContactShiftAttr}")
        #zPosDefault = cmds.getAttr(f"{self.root}.{self.zPosDefaultAttr}")
        scaleFactor = cmds.getAttr(f"{self.root}.{self.scaleFactorAttr}")

        cmds.cutKey(f"{root}.{self.fpsAttr}", cl=True)
        cmds.cutKey(f"{root}.{self.idleAttr}", cl=True)
        cmds.cutKey(f"{root}.{self.forwardAttr}", cl=True)
        cmds.cutKey(f"{root}.{self.backwardAttr}", cl=True)
        cmds.cutKey(f"{root}.{self.locomotionAttr}", cl=True)
        cmds.cutKey(f"{root}.{self.magnitudeAttr}", cl=True)
        cmds.cutKey(f"{root}.{self.magnitudeSmoothAttr}", cl=True)
        cmds.cutKey(f"{root}.{self.magnitudeMeanAttr}", cl=True)
        cmds.cutKey(f"{root}.{self.feetOffsetAttr}", cl=True)
        cmds.cutKey(f"{root}.{self.footContactAttrs['l']}", cl=True)
        cmds.cutKey(f"{root}.{self.footContactAttrs['r']}", cl=True)
        #cmds.cutKey(f"{root}.{self.zPosDefaultAttr}", cl=True)
        cmds.cutKey(f"{root}.{self.scaleFactorAttr}", cl=True)

        cmds.setKeyframe(root, at=self.fpsAttr, t=start, v=fps)

        type = self.type if self.type <= 1 else 0
        cmds.setKeyframe(root, at=self.idleAttr, t=start, v=1-abs(type))
        forwardVal = 0 if self.type != 1 else 1
        cmds.setKeyframe(root, at=self.forwardAttr, t=start, v=forwardVal)
        backwardVal = 0 if self.type != -1 else 1
        cmds.setKeyframe(root, at=self.backwardAttr, t=start, v=backwardVal)

        cmds.copyKey(root, at=self.dir, t=(start, end), o='curve')
        cmds.pasteKey(root, at=self.locomotionAttr, o='replaceCompletely')
        if scaleFactor != 1:
            cmds.scaleKey(root, at=self.locomotionAttr, an="objects", t=(start, end), vp=0, vs=1./scaleFactor)

        rootValZPre = 0.
        magnitude = 0.
        magnitudeMean = 0.

        for frame in range(start, end + 1):
            cmds.currentTime(frame)

            rootVal = cmds.xform(self.root, q=True, t=True, ws=True)
            footLVal = cmds.xform(self.feet['l'], q=True, t=True, ws=True)
            footRVal = cmds.xform(self.feet['r'], q=True, t=True, ws=True)

            rootValDir = rootVal[2]
            if self.dir == 'translateX':
                rootValDir = rootVal[0]
            if frame > start:
                if self.type == -1 or self.type == 1:
                    magnitude = (rootValDir - rootValZPre) * fps
                    if scaleFactor != 1:
                        magnitude /= scaleFactor
                else:
                    magnitude = 0.
                cmds.setKeyframe(f"{root}.{self.magnitudeAttr}", itt='linear', ott='linear', t=frame, value=magnitude)
                magnitudeMean += magnitude
            rootValZPre = rootValDir

            footContL = 0
            footContR = 0
            if footLVal[1] < feetOffsetDefault + feetThresholds[0]:
                footContL = 1
            if footRVal[1] < feetOffsetDefault + feetThresholds[0]:
                footContR = 1
            cmds.setKeyframe(f"{root}.{self.footContactAttrs['l']}", itt='linear', ott='linear', t=frame, v=footContL)
            cmds.setKeyframe(f"{root}.{self.footContactAttrs['r']}", itt='linear', ott='linear', t=frame, v=footContR)

        #if self.type == 0 or self.type == 10:
        #    cmds.currentTime(start)
        #    cmds.cutKey(f'{root}.{self.magnitudeAttr}', t=(start + 2, end + 1000000), cl=True)
        #    cmds.setAttr(f'{root}.{self.magnitudeAttr}', 0)
        #    cmds.setKeyframe(root, an=False, at=self.magnitudeAttr, t=start, v=0)
        #    cmds.keyframe(root, at=self.magnitudeAttr, animation='keys', absolute=True, valueChange=0)

        cmds.copyKey(root, at=self.magnitudeAttr, t=(start, end), o='curve')
        cmds.pasteKey(root, at=self.magnitudeSmoothAttr, o='replaceCompletely')
        cmds.keyTangent(root, at=self.magnitudeSmoothAttr, itt='auto', ott='auto', animation='objects')
        if self.type == 0 or self.type == 10:
            cmds.cutKey(f'{root}.{self.magnitudeSmoothAttr}', t=(start + 1, end + 1000000), cl=True)
        if self.type != 0 and self.type != 10:
            cmds.filterCurve(f'{root}.{self.magnitudeSmoothAttr}', f='butterworth', cof=6, sr=9, s=0, e=122)

        cmds.setKeyframe(f'{root}.{self.magnitudeAttr}', itt='linear', ott='linear', time=start, value=magnitude)
        magnitudeMean += magnitude
        magnitudeMean /= end + 1
        cmds.setKeyframe(root, at=self.magnitudeMeanAttr, t=start, v=magnitudeMean)

        footContactStackL = [0. for _ in range(7)]
        footContactStackR = [0. for _ in range(7)]
        for frame in range(start, end + 1):
            cmds.currentTime(frame)

            footContactValL = cmds.getAttr(f"{root}.{self.footContactAttrs['l']}")
            footContactValR = cmds.getAttr(f"{root}.{self.footContactAttrs['r']}")
            footContactStackL.pop(0)
            footContactStackR.pop(0)
            footContactStackL.append(footContactValL)
            footContactStackR.append(footContactValR)

            TOLERANCE = 0.99999
            if statistics.mean(footContactStackL) >= TOLERANCE:
                toeLVal = cmds.xform(self.toes['l'], q=True, t=True, ws=True)
                footContL = 1
                if toeLVal[1] >= toesOffsetDefault + toesThreshold:
                    footContL = 0
                cmds.setKeyframe(f"{root}.{self.footContactAttrs['l']}", itt='linear', ott='linear', t=frame, v=footContL)
            if statistics.mean(footContactStackR) >= TOLERANCE:
                toeRVal = cmds.xform(self.toes['r'], q=True, t=True, ws=True)
                footContR = 1
                if toeRVal[1] >= toesOffsetDefault + toesThreshold:
                    footContR = 0
                cmds.setKeyframe(f"{root}.{self.footContactAttrs['r']}", itt='linear', ott='linear', t=frame, v=footContR)

        if footContactResamplePeriod != 1.:
            cmds.filterCurve(f"{root}.{self.footContactAttrs['l']}", f='resample', ker=self.filterKernels[footContactResampleKernel], per=footContactResamplePeriod)
            cmds.filterCurve(f"{root}.{self.footContactAttrs['r']}", f='resample', ker=self.filterKernels[footContactResampleKernel], per=footContactResamplePeriod)
            cmds.setKeyframe(root, at=self.footContactAttrs['l'], an=True, i=True, t=end)
            cmds.setKeyframe(root, at=self.footContactAttrs['r'], an=True, i=True, t=end)
            cmds.cutKey(root, at=self.footContactAttrs['l'], an='objects', cl=True, t=(end + 1, end + 1000000))
            cmds.cutKey(root, at=self.footContactAttrs['r'], an='objects', cl=True, t=(end + 1, end + 1000000))

            if footContactReduction:
                cmds.filterCurve(f"{root}.{self.footContactAttrs['l']}", f='keyreducer', pm=1, pre=0, s=start, e=end)
                cmds.filterCurve(f"{root}.{self.footContactAttrs['r']}", f='keyreducer', pm=1, pre=0, s=start, e=end)

            for frame in range(start + 1, end):
                cmds.setKeyframe(root, at=self.footContactAttrs['l'], an=True, i=True, t=frame)
                cmds.setKeyframe(root, at=self.footContactAttrs['r'], an=True, i=True, t=frame)
            cmds.keyTangent(root, at=self.footContactAttrs['l'], itt='auto', ott='auto', an='objects')
            cmds.keyTangent(root, at=self.footContactAttrs['r'], itt='auto', ott='auto', an='objects')

        if footContactShift != 0:
           cmds.keyframe(root, attribute=f"{self.footContactAttrs['l']}", edit=True, r=True, tc=footContactShift, t=(start, end))
           cmds.keyframe(root, attribute=f"{self.footContactAttrs['r']}", edit=True, r=True, tc=footContactShift, t=(start, end))
           cmds.cutKey(root, at=f"{self.footContactAttrs['l']}", cl=True, t=(start - 1000000, start - 1))
           cmds.cutKey(root, at=f"{self.footContactAttrs['r']}", cl=True, t=(start - 1000000, start - 1))
           cmds.cutKey(root, at=f"{self.footContactAttrs['l']}", cl=True, t=(end + 1, end + 1000000))
           cmds.cutKey(root, at=f"{self.footContactAttrs['r']}", cl=True, t=(end + 1, end + 1000000))

        cmds.currentTime(start)

        offset = feetOffsetDefault
        if scaleFactor != 1:
            offset /= scaleFactor
        #cmds.setAttr(f'{root}.{self.feetOffsetAttr}', offset)
        cmds.setKeyframe(root, at=self.feetOffsetAttr, t=start, v=offset)

        ats = []

        if self.type == -1 or self.type == 1:
            cmds.cutKey(root, at=self.dir, cl=True)
            cmds.setAttr(f'{root}.{self.dir}', 0)
            ats.append(self.dir)
        elif self.type == 10:
            #cmds.cutKey(root, at='rotateX', cl=True)
            #cmds.setAttr(f'{root}.rotateX', 0)
            #ats.append('rotateX')
            pass

        cmds.setKeyframe(root, at=self.scaleFactorAttr, t=start)

        if footContactShift != 0:
            ats.extend([self.footContactAttrs['l'], self.footContactAttrs['r']])
        if bake:
            ats.extend([self.fpsAttr, self.idleAttr, self.forwardAttr, self.backwardAttr, self.magnitudeMeanAttr, self.feetOffsetAttr, self.scaleFactorAttr])
        if len(ats) > 0:
            print(f"Baking: {ats}")
            cmds.bakeResults(root, at=ats, sm=True, t=(start, end), sb=1, osr=1, dic=True, pok=True, sac=False, ral=False, rba=False, bol=False, mr=True)

    # def animLayers(self, on=1):
    #     cmds.animLayer('Root', edit=True, weight=on, mute=1-on)
    #     cmds.animLayer('BaseAnimation', edit=True, weight=on, mute=1-on)
