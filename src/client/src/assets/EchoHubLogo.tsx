interface EchoHubLogoProps {
  className?: string;
}

export default function EchoHubLogo({ className }: EchoHubLogoProps) {
  return (
    <svg
      className={className}
      viewBox="0 0 1000 1000"
      xmlns="http://www.w3.org/2000/svg"
      aria-label="EchoHub logo"
    >
      <defs>
        <linearGradient id="logoGradient" x1="0%" y1="0%" x2="100%" y2="0%">
          <stop offset="0%" stopColor="#7b19e4" />
          <stop offset="100%" stopColor="#f8af8e" />
        </linearGradient>
      </defs>
      <g stroke="url(#logoGradient)" strokeWidth="28" fill="none" strokeLinecap="round" strokeLinejoin="round">
        <circle cx="500" cy="500" r="420" />
        <circle cx="500" cy="500" r="180" />
        <polygon points="500,440 550,532 450,532" fill="url(#logoGradient)" />
        <line x1="500" y1="80" x2="500" y2="320" />
        <circle cx="285" cy="390" r="8" fill="url(#logoGradient)" />
        <circle cx="715" cy="390" r="8" fill="url(#logoGradient)" />
        <circle cx="500" cy="750" r="8" fill="url(#logoGradient)" />
      </g>
    </svg>
  );
}
