export interface TagColor {
  bg: string;
  border: string;
  text: string;
}

function hashString(value: string): number {
  let h = 2166136261;
  for (let i = 0; i < value.length; i++) {
    h ^= value.charCodeAt(i);
    h = Math.imul(h, 16777619);
  }
  return h >>> 0;
}

export function tagColor(tag: string): TagColor {
  const hue = hashString(tag.toLowerCase()) % 360;
  return {
    bg: `hsla(${hue}, 75%, 55%, 0.18)`,
    border: `hsla(${hue}, 75%, 60%, 0.55)`,
    text: `hsl(${hue}, 85%, 78%)`,
  };
}
