# click360.py
# pip install opencv-python
import cv2, os, sys

path = "Placeholder/Assets/Images/pi_IMG_20180703_153253_109.jpg"
img  = cv2.imread(path)
if img is None: raise FileNotFoundError(path)
h,w  = img.shape[:2]
pts  = []

def click(event, x, y, flags, param):
    if event == cv2.EVENT_LBUTTONDOWN:
        pts.append((x/w, y/h))          # normalise immediately
        print(f"pt{len(pts)}  u={x/w:.3f}  v={y/h:.3f}")
        cv2.circle(img, (x,y), 3, (0,0,255), -1)
        cv2.imshow('img', img)
    elif event == cv2.EVENT_RBUTTONDOWN and len(pts)>=3:
        out = '|'.join(f"{u},{1.0-v}" for u,v in pts)
        print("\nCOPY THIS LINE →", out)
        pts.clear()

cv2.namedWindow('img', cv2.WINDOW_NORMAL)
cv2.setMouseCallback('img', click)
cv2.imshow('img', img)
cv2.waitKey(0)
cv2.destroyAllWindows()