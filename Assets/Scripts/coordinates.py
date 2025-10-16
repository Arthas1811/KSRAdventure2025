import cv2
path = "/Users/leonsieber/360 Demo/Placeholder/Assets/Images/pi_IMG_20180703_154205_115.jpg"
image = cv2.imread(path)
height, width = image.shape[:2]
points = []

def click(event, x, y, useless_one, useless_two):
    if event == cv2.EVENT_LBUTTONDOWN:
        points.append((x/width, y/height))
        cv2.circle(image, (x, y), 20, (0, 0, 255), -1)
        cv2.imshow("img", image)
    elif event == cv2.EVENT_RBUTTONDOWN and len(points) >= 3:
        output = ""
        for x, y in points:
            if output == "":
                output += (str(x) + "," + str(1-y))
            else:
                output += (";" + str(x) + "," + str(1-y))
        print(output)
        points.clear()

cv2.imshow("img", image)
cv2.setMouseCallback("img", click)
cv2.waitKey(0)
cv2.destroyAllWindows()