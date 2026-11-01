import { inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ScreenService } from '../services/screen.service';

@Injectable({ providedIn: 'root' })
export class ScreenSessionService {
  private readonly screenApi = inject(ScreenService);

  active = signal(false);
  frameCount = signal(0);
  shareStopped = signal(false);

  private stream: MediaStream | null = null;
  private video: HTMLVideoElement | null = null;
  private timer: number | null = null;
  private pending = false;
  private closing = false;

  async begin(intervalMinutes: number): Promise<void> {
    this.stopTracks();
    const stream = await this.openEntireScreen();
    this.stream = stream;
    this.closing = false;
    this.frameCount.set(0);
    this.shareStopped.set(false);
    this.pending = true;
    this.active.set(true);

    const video = document.createElement('video');
    video.srcObject = stream;
    video.muted = true;
    video.playsInline = true;
    video.autoplay = true;
    video.setAttribute('aria-hidden', 'true');
    video.style.cssText = 'position:fixed;width:1px;height:1px;opacity:0;pointer-events:none;left:0;top:0';
    document.body.appendChild(video);
    this.video = video;
    await video.play();
    await this.waitForFrame(video);

    stream.getVideoTracks()[0]?.addEventListener('ended', () => {
      if (this.closing) return;
      this.shareStopped.set(true);
      this.clearTimer();
    });

    await this.capture();
    const minutes = Math.min(60, Math.max(1, intervalMinutes));
    this.timer = window.setInterval(() => void this.capture(), minutes * 60 * 1000);
  }

  isPending(): boolean {
    return this.pending;
  }

  cancel(): void {
    this.pending = false;
    this.frameCount.set(0);
    this.active.set(false);
    this.shareStopped.set(false);
    this.stopTracks();
  }

  stopSharing(): void {
    this.stopTracks();
    this.active.set(false);
  }

  clear(): void {
    this.pending = false;
    this.frameCount.set(0);
    this.shareStopped.set(false);
  }

  private async openEntireScreen(): Promise<MediaStream> {
    if (!navigator.mediaDevices?.getDisplayMedia) {
      throw new Error('This browser cannot share a screen, so clock in was not started.');
    }

    let stream: MediaStream;
    try {
      stream = await navigator.mediaDevices.getDisplayMedia({
        video: {
          displaySurface: 'monitor',
          width: { ideal: 1920 },
          height: { ideal: 1080 }
        } as MediaTrackConstraints,
        audio: false
      });
    } catch {
      throw new Error('Screen sharing was cancelled, so clock in was not started.');
    }

    const surface = stream.getVideoTracks()[0]?.getSettings().displaySurface;
    if (surface !== 'monitor') {
      stream.getTracks().forEach(track => track.stop());
      throw new Error('Share your entire screen. A window or browser tab cannot be used to clock in.');
    }
    return stream;
  }

  private waitForFrame(video: HTMLVideoElement): Promise<void> {
    if (video.videoWidth > 0) return Promise.resolve();
    return new Promise((resolve, reject) => {
      const started = window.setInterval(() => {
        if (video.videoWidth > 0) {
          window.clearInterval(started);
          window.clearTimeout(giveUp);
          resolve();
        }
      }, 100);
      const giveUp = window.setTimeout(() => {
        window.clearInterval(started);
        reject(new Error('The shared screen did not produce an image, so clock in was not started.'));
      }, 5000);
    });
  }

  private async capture(): Promise<void> {
    const video = this.video;
    if (!video) return;
    if (!video.videoWidth) {
      try {
        await this.waitForFrame(video);
      } catch {
        return;
      }
    }
    const canvas = document.createElement('canvas');
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    canvas.getContext('2d')?.drawImage(video, 0, 0);
    const image = await new Promise<Blob | null>(resolve => canvas.toBlob(resolve, 'image/jpeg', 0.92));
    if (!image) return;
    try {
      await firstValueFrom(this.screenApi.uploadFrame(image, new Date().toISOString()));
      this.frameCount.update(count => count + 1);
    } catch (err) {
      if (this.frameCount() === 0) throw err;
    }
  }

  private clearTimer(): void {
    if (this.timer != null) window.clearInterval(this.timer);
    this.timer = null;
  }

  private stopTracks(): void {
    this.closing = true;
    this.clearTimer();
    this.video?.pause();
    this.video?.remove();
    this.video = null;
    this.stream?.getTracks().forEach(track => track.stop());
    this.stream = null;
  }
}
