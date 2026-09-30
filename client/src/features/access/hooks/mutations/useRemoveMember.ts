import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import type { ApiError } from "@/types/apiError.types";
import { deleteMember, rejectMember } from "../../api/access.api";
import { accessKeys } from "../queries/useMembers";
import type { MemberList } from "../../types/access.types";
import { countMembers } from "./useOptimisticMemberMutation";

/** Destructive, so no optimism: the row only leaves the list once the server confirms. */
function useRemoveMember<TVars extends { id: string }>(mutationFn: (vars: TVars) => Promise<void>, errorMessage: string) {
  const queryClient = useQueryClient();

  return useMutation<void, ApiError, TVars>({
    mutationFn,
    onSuccess: (_data, { id }) => {
      queryClient.setQueryData<MemberList>(accessKeys.members(), (old) => {
        if (!old) return old;
        const items = old.items.filter((m) => m.id !== id);
        return { items, counts: countMembers(items) };
      });
      queryClient.invalidateQueries({ queryKey: accessKeys.all });
    },
    onError: (error) => toast.error(error.message ?? errorMessage),
  });
}

export const useRejectMember = () =>
  useRemoveMember<{ id: string; reason: string }>(({ id, reason }) => rejectMember(id, reason), "Couldn't reject this request.");

export const useDeleteMember = () =>
  useRemoveMember<{ id: string }>(({ id }) => deleteMember(id), "Couldn't delete this member.");
