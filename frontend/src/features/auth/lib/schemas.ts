import { z } from "zod";

/**
 * Política de senha RN09 (docs/02-regras-de-negocio.md), espelhando
 * RegistrarEmpresaCommandValidator (Etapa 9.8): 8+ caracteres, ao menos
 * uma maiúscula e um número. Duplicada de propósito no cliente para dar
 * feedback imediato - a fonte de verdade continua sendo o backend.
 */
const senhaSchema = z
  .string()
  .min(8, "A senha deve ter ao menos 8 caracteres.")
  .regex(/[A-Z]/, "A senha deve conter ao menos uma letra maiúscula.")
  .regex(/[0-9]/, "A senha deve conter ao menos um número.");

export const loginSchema = z.object({
  email: z.string().min(1, "Informe o e-mail.").email("E-mail inválido."),
  senha: z.string().min(1, "Informe a senha.")
});

export type LoginFormValues = z.infer<typeof loginSchema>;

export const registrarEmpresaSchema = z
  .object({
    nomeEmpresa: z.string().min(1, "O nome da empresa é obrigatório.").max(200),
    cnpjOuIdentificador: z.string().max(20).optional().or(z.literal("")),
    nomeAdministrador: z.string().min(1, "O nome é obrigatório.").max(150),
    email: z.string().min(1, "Informe o e-mail.").email("E-mail inválido."),
    senha: senhaSchema,
    confirmarSenha: z.string().min(1, "Confirme a senha.")
  })
  .refine((dados) => dados.senha === dados.confirmarSenha, {
    message: "As senhas não coincidem.",
    path: ["confirmarSenha"]
  });

export type RegistrarEmpresaFormValues = z.infer<typeof registrarEmpresaSchema>;

export const esqueciSenhaSchema = z.object({
  email: z.string().min(1, "Informe o e-mail.").email("E-mail inválido.")
});

export type EsqueciSenhaFormValues = z.infer<typeof esqueciSenhaSchema>;

export const redefinirSenhaSchema = z
  .object({
    token: z.string().min(1),
    novaSenha: senhaSchema,
    confirmarSenha: z.string().min(1, "Confirme a senha.")
  })
  .refine((dados) => dados.novaSenha === dados.confirmarSenha, {
    message: "As senhas não coincidem.",
    path: ["confirmarSenha"]
  });

export type RedefinirSenhaFormValues = z.infer<typeof redefinirSenhaSchema>;