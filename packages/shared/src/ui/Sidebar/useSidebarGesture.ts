import { useRef, useState } from 'react';
import type { PointerEvent } from 'react';

const SIDEBAR_WIDTH = 280;
const MIN_DELTA = 50;
const HORIZONTAL_RATIO = 1.5;
const OPEN_THRESHOLD = 0.5;

interface GestureStart {
  x: number;
  y: number;
  wasOpen: boolean;
}

export function useSidebarGesture() {
  const [isOpen, setIsOpen] = useState(false);
  const [dragOffset, setDragOffset] = useState(0);
  const [isDragging, setIsDragging] = useState(false);
  const gestureStart = useRef<GestureStart | null>(null);
  const isHorizontal = useRef(false);

  const onPointerDown = (event: PointerEvent<HTMLElement>) => {
    if (window.matchMedia('(min-width: 1024px)').matches) return;
    if (event.pointerType === 'mouse' && event.button !== 0) return;
    if (!(event.target instanceof HTMLElement)) return;
    if (event.target.closest('[data-no-swipe], input, textarea, select, [contenteditable="true"]')) return;
    if (window.getSelection()?.isCollapsed === false) return;

    gestureStart.current = { x: event.clientX, y: event.clientY, wasOpen: isOpen };
    isHorizontal.current = false;
  };

  const onPointerMove = (event: PointerEvent<HTMLElement>) => {
    const start = gestureStart.current;
    if (!start) return;

    const deltaX = event.clientX - start.x;
    const deltaY = event.clientY - start.y;

    if (!isHorizontal.current) {
      if (Math.abs(deltaX) < 10) return;
      if (Math.abs(deltaX) > Math.abs(deltaY) * HORIZONTAL_RATIO) {
        isHorizontal.current = true;
        setIsDragging(true);
      } else {
        gestureStart.current = null;
        return;
      }
    }

    const progress = (start.wasOpen ? SIDEBAR_WIDTH : 0) + deltaX;
    setDragOffset(Math.min(1, Math.max(0, progress / SIDEBAR_WIDTH)));
  };

  const finishGesture = (event: PointerEvent<HTMLElement>) => {
    const start = gestureStart.current;
    if (!start) return;

    const deltaX = event.clientX - start.x;
    if (isHorizontal.current && Math.abs(deltaX) >= MIN_DELTA) {
      const progress = (start.wasOpen ? SIDEBAR_WIDTH : 0) + deltaX;
      setIsOpen(progress / SIDEBAR_WIDTH > OPEN_THRESHOLD);
    }

    gestureStart.current = null;
    isHorizontal.current = false;
    setIsDragging(false);
    setDragOffset(0);
  };

  const onPointerCancel = () => {
    gestureStart.current = null;
    isHorizontal.current = false;
    setIsDragging(false);
    setDragOffset(0);
  };

  return {
    isOpen,
    dragOffset,
    isDragging,
    open: () => setIsOpen(true),
    close: () => setIsOpen(false),
    handlers: {
      onPointerDown,
      onPointerMove,
      onPointerUp: finishGesture,
      onPointerCancel,
    },
  };
}
