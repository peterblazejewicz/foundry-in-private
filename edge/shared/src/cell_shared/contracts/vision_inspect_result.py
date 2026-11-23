# Generated from contracts/vision-inspect-result.schema.json by contracts/generate_python.py.
# Do not edit. Change the schema and run the generator again.

from __future__ import annotations

from typing import Annotated

from pydantic import AnyUrl, AwareDatetime, BaseModel, ConfigDict, Field, RootModel


class ResolutionItem(RootModel[int]):
    root: Annotated[int, Field(ge=1)]


class Camera(BaseModel):
    model_config = ConfigDict(
        extra="forbid",
    )
    model: Annotated[str, Field(min_length=1)]
    resolution: Annotated[list[ResolutionItem], Field(max_length=2, min_length=2)]
    """
    Frame size in pixels: [width, height].
    """
    intelligent_zoom: bool


class Detector(BaseModel):
    model_config = ConfigDict(
        extra="forbid",
    )
    model: Annotated[str, Field(min_length=1)]
    version: Annotated[str, Field(min_length=1)]
    runtime: Annotated[str, Field(min_length=1)]
    latency_ms: Annotated[float, Field(ge=0.0)]


class BboxXyxyItem(RootModel[int]):
    root: Annotated[int, Field(ge=0)]


class Detection(BaseModel):
    model_config = ConfigDict(
        extra="forbid",
    )
    label: Annotated[str, Field(min_length=1)]
    confidence: Annotated[float, Field(ge=0.0, le=1.0)]
    bbox_xyxy: Annotated[list[BboxXyxyItem], Field(max_length=4, min_length=4)]
    """
    Box in full-frame pixels: [x_min, y_min, x_max, y_max], with x_max > x_min and y_max > y_min.
    """


class Images(BaseModel):
    model_config = ConfigDict(
        extra="forbid",
    )
    full: AnyUrl
    """
    LAN URL of the full frame on board A.
    """
    crops: list[AnyUrl]
    """
    LAN URLs of the crops on board A, one per detection, in the same order.
    """


class VisionInspectResult(BaseModel):
    """
    Result of the vision.inspect MCP tool (architecture §7.2), board A to board B.
    """

    model_config = ConfigDict(
        extra="forbid",
    )
    inspection_id: Annotated[str, Field(max_length=128, pattern="^insp-[A-Za-z0-9-]+$")]
    """
    Correlation key of one inspection.
    """
    part_id: Annotated[str, Field(max_length=64, min_length=1)]
    """
    Identifier of the inspected part.
    """
    captured_at: AwareDatetime
    """
    Capture time in UTC, RFC 3339 with a Z offset.
    """
    camera: Camera
    detector: Detector
    detections: list[Detection]
    """
    Detections in descending confidence order. Empty when the detector found nothing.
    """
    images: Images
