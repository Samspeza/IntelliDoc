import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

/**
 * Combina classes Tailwind condicionalmente (clsx) e resolve conflitos de
 * utilitário (tailwind-merge) - ex.: cn("px-2", condicao && "px-4") sempre
 * resulta em "px-4" quando condicao é true, em vez de ambas as classes
 * concatenadas (o que o CSS resolveria de forma imprevisível por ordem de
 * declaração). Padrão usado por todos os componentes Shadcn UI.
 */
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}