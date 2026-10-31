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
    await video.play();
    this.video = video;

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

  private async capture(): Promise<void> {
    const video = this.video;
    if (!video || !video.videoWidth) return;
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
    this.video = null;
    this.stream?.getTracks().forEach(track => track.stop());
    this.stream = null;
  }
}
